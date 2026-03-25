// <copyright file="AntiIdlenessService.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace EleveRats.Services;

/// <summary>
/// Background service to maintain CPU and RAM usage above Oracle's 10% threshold.
/// Performs parallel AES-256-GCM encryption and dynamic memory allocation.
/// Optimized for ARM64 high-efficiency environments in 2026.
/// </summary>
internal sealed class AntiIdlenessService : BackgroundService
{
    // Alvo: Manter ~3GB de RAM ocupados (3000 entradas de ~1MB cada)
    private const int _targetRamEntries = 3000;
    private const int _batchDeleteSize = 500;

    private readonly ConcurrentDictionary<string, byte[]> _storage = new();

    private readonly string[] _loremWords =
        "lorem ipsum dolor sit amet consectetur adipiscing elit".Split(' ');

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Alinhado ao manifesto: Uma saudação visceral para o início da jornada.
        Console.WriteLine("Dolphin Wake-up Service: Entering high-performance mode... 🐬🔥");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Paralelismo real: ocupa todos os núcleos da CPU ARM
                await this.PerformHeavyLifting();

                // Delay reduzido drasticamente para 5ms para manter a CPU aquecida
                await Task.Delay(5, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
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
            Parallel.For(
                0,
                Environment.ProcessorCount,
                _ =>
                {
                    // 2. Geração de Dados Aumentada: ~1MB por iteração
                    // Criar textura e peso real para a memória
                    int length = RandomNumberGenerator.GetInt32(10000, 20000);
                    var sb = new StringBuilder();
                    for (int i = 0; i < length; i++)
                    {
                        sb.Append(
                            this._loremWords[
                                RandomNumberGenerator.GetInt32(this._loremWords.Length)
                            ] + " "
                        );
                    }

                    byte[] plaintext = Encoding.UTF8.GetBytes(sb.ToString());
                    byte[] key = new byte[32];
                    byte[] iv = new byte[12];
                    byte[] tag = new byte[16];
                    byte[] ciphertext = new byte[plaintext.Length];

                    RandomNumberGenerator.Fill(key);
                    RandomNumberGenerator.Fill(iv);

                    // 3. Criptografia Sem Dó: Força o JIT a trabalhar no limite do container
                    using (var aesGcm = new AesGcm(key, tag.Length))
                    {
                        aesGcm.Encrypt(iv, plaintext, ciphertext, tag);
                    }

                    // 4. Alocação de Memória: Criando "gordura" para o monitor da Oracle ver
                    string entryKey = $"key_{Guid.NewGuid()}";
                    this._storage.TryAdd(entryKey, ciphertext);
                }
            );
        });

        // 5. Cleanup Dinâmico: Mantém a ocupação alta sem estourar o limite de 4GB do Docker
        if (this._storage.Count >= _targetRamEntries)
        {
            var keysToDelete = this._storage.Keys.Take(_batchDeleteSize).ToList();
            foreach (string k in keysToDelete)
            {
                this._storage.TryRemove(k, out _);
            }

            // Força um yield para o sistema operacional respirar e o GC agir se necessário
            await Task.Yield();
        }
    }
}
