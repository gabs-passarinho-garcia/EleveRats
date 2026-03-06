using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;

namespace EleveRats.Services;

/// <summary>
/// Background service to maintain CPU and RAM usage above Oracle's 10% threshold.
/// Performs parallel AES-256-GCM encryption and dynamic memory allocation.
/// Optimized for ARM64 high-efficiency environments in 2026.
/// </summary>
public class AntiIdlenessService : BackgroundService
{
    private readonly ConcurrentDictionary<string, byte[]> _storage = new();
    
    // Alvo: Manter ~3GB de RAM ocupados (3000 entradas de ~1MB cada)
    private const int TargetRamEntries = 3000; 
    private const int BatchDeleteSize = 500;
    
    private readonly string[] _loremWords = "lorem ipsum dolor sit amet consectetur adipiscing elit".Split(' ');

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Alinhado ao manifesto: Uma saudação visceral para o início da jornada.
        Console.WriteLine("Dolphin Wake-up Service: Entering high-performance mode... 🐬🔥");

        while (!stoppingToken.IsCancellationRequested)
        {
            try 
            {
                // Paralelismo real: ocupa todos os núcleos da CPU ARM
                await PerformHeavyLifting();
                
                // Delay reduzido drasticamente para 5ms para manter a CPU aquecida
                await Task.Delay(5, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                // Sábio e direto: logs limpos para problemas reais
                Console.WriteLine($"[AntiIdleness] Unexpected friction: {ex.Message}");
            }
        }
    }

    private async Task PerformHeavyLifting()
    {
        // 1. Paralelismo: Ocupa os 4 núcleos físicos da Ampere
        await Task.Run(() => 
        {
            Parallel.For(0, Environment.ProcessorCount, _ => 
            {
                // 2. Geração de Dados Aumentada: ~1MB por iteração
                // Criar textura e peso real para a memória
                var length = RandomNumberGenerator.GetInt32(10000, 20000);
                var sb = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    sb.Append(_loremWords[RandomNumberGenerator.GetInt32(_loremWords.Length)] + " ");
                }

                var plaintext = Encoding.UTF8.GetBytes(sb.ToString());
                var key = new byte[32]; 
                var iv = new byte[12];  
                var tag = new byte[16];
                var ciphertext = new byte[plaintext.Length];

                RandomNumberGenerator.Fill(key);
                RandomNumberGenerator.Fill(iv);

                // 3. Criptografia Sem Dó: Força o JIT a trabalhar no limite do container
                using (var aesGcm = new AesGcm(key, tag.Length))
                {
                    aesGcm.Encrypt(iv, plaintext, ciphertext, tag);
                }

                // 4. Alocação de Memória: Criando "gordura" para o monitor da Oracle ver
                var entryKey = $"key_{Guid.NewGuid()}";
                _storage.TryAdd(entryKey, ciphertext);
            });
        });

        // 5. Cleanup Dinâmico: Mantém a ocupação alta sem estourar o limite de 4GB do Docker
        if (_storage.Count >= TargetRamEntries)
        {
            var keysToDelete = _storage.Keys.Take(BatchDeleteSize).ToList();
            foreach (var k in keysToDelete)
            {
                _storage.TryRemove(k, out _);
            }
            
            // Força um yield para o sistema operacional respirar e o GC agir se necessário
            await Task.Yield(); 
        }
    }
}