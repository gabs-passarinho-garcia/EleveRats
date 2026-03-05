using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;

namespace EleveRats.Services;

/// <summary>
/// Background service to maintain CPU and RAM usage above Oracle's 10% threshold.
/// Performs continuous AES-256-GCM encryption on random data.
/// </summary>
public class AntiIdlenessService : BackgroundService
{
    private readonly ConcurrentDictionary<string, byte[]> _storage = new();
    private const int IterationLimit = 1000;
    private const int DeleteCount = 500;
    private readonly string[] _loremWords = "lorem ipsum dolor sit amet consectetur adipiscing elit".Split(' ');

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Dolphin Wake-up Service started! 🐬");

        while (!stoppingToken.IsCancellationRequested)
        {
            try 
            {
                await PerformHeavyLifting();
                // Adjust delay to control CPU impact (100ms = ~10 iterations/sec)
                await Task.Delay(100, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task PerformHeavyLifting()
    {
        // 1. Generate random Lorem Ipsum
        var length = RandomNumberGenerator.GetInt32(100, 600);
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(_loremWords[RandomNumberGenerator.GetInt32(_loremWords.Length)] + " ");
        }

        var plaintext = Encoding.UTF8.GetBytes(sb.ToString());
        var key = new byte[32]; // AES-256
        var iv = new byte[12];  // GCM standard nonce
        var tag = new byte[16];
        var ciphertext = new byte[plaintext.Length];

        RandomNumberGenerator.Fill(key);
        RandomNumberGenerator.Fill(iv);

        // 2. AES-256-GCM Encryption (Heavy CPU task)
        using (var aesGcm = new AesGcm(key, tag.Length))
        {
            aesGcm.Encrypt(iv, plaintext, ciphertext, tag);
        }

        // 3. Memory storage (Occupies RAM)
        var entryKey = $"key_{Guid.NewGuid()}";
        _storage.TryAdd(entryKey, ciphertext);

        // 4. Cleanup logic
        if (_storage.Count >= IterationLimit)
        {
            var keysToDelete = _storage.Keys.Take(DeleteCount).ToList();
            keysToDelete.ForEach(k => _storage.TryRemove(k, out _));
            
            // Minimal logging to keep it "dumb" but visible
            await Task.Yield(); 
        }
    }
}
