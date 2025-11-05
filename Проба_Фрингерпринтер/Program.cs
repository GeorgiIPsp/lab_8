using System;
using System.IO;
using System.Diagnostics;
using Проба_Фрингерпринтер.Classes;


namespace Mfccextractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var fingerprinter = new MfccFingerprinter();
            bool isRunning = true;

            while (isRunning)
            {
                Console.Clear();
                Console.WriteLine("=== Аудио фингерпринтинг ===");
                Console.WriteLine("1. Генерация фингерпринта из файла");
                Console.WriteLine("2. Сравнение двух аудиофайлов");
                Console.WriteLine("3. Выход");
                Console.Write("Выберите действие: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        GenerateFingerprintMenu(fingerprinter);
                        break;
                    case "2":
                        CompareFilesMenu(fingerprinter);
                        break;
                    case "3":
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите любую клавишу...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void GenerateFingerprintMenu(MfccFingerprinter fingerprinter)
        {
            Console.Clear();
            Console.WriteLine("=== Генерация фингерпринта ===");

            string filePath = GetAudioFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Файл не выбран. Нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }

            try
            {
                string plotPath = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                   "audio_waveform.png");
                Console.WriteLine("Идет обработка...");
                byte[] fingerprint = fingerprinter.GenerateFingerprint(filePath);
                Console.WriteLine($"Фингерпринт успешно создан! Размер: {fingerprint.Length} байт");
                fingerprinter.PlotSingleAudioWaveform(filePath, plotPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static void CompareFilesMenu(MfccFingerprinter fingerprinter)
        {
            Console.Clear();
            Console.WriteLine("=== Сравнение аудиофайлов ===");

            Console.WriteLine("Выберите первый файл:");
            string filePath1 = GetAudioFilePath();
            if (string.IsNullOrEmpty(filePath1))
            {
                Console.WriteLine("Первый файл не выбран. Нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nВыберите второй файл:");
            string filePath2 = GetAudioFilePath();
            if (string.IsNullOrEmpty(filePath2))
            {
                Console.WriteLine("Второй файл не выбран. Нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }

            try
            {
                Console.WriteLine("\nИдет обработка...");

                // Генерация фингерпринтов
                byte[] fp1 = fingerprinter.GenerateFingerprint(filePath1);
                byte[] fp2 = fingerprinter.GenerateFingerprint(filePath2);

                // Сравнение
                double similarity = fingerprinter.Compare(fp1, fp2);

                // Визуализация
                string plotPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "audio_comparison.png");

                fingerprinter.PlotAudioWaveform(filePath1, filePath2, plotPath);

                Console.WriteLine($"\nРезультат сравнения:");
                Console.WriteLine($"Файл 1: {Path.GetFileName(filePath1)}");
                Console.WriteLine($"Файл 2: {Path.GetFileName(filePath2)}");
                Console.WriteLine($"Схожесть: {similarity:F2}%");
                Console.WriteLine($"График сохранен: {plotPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }


        static string GetAudioFilePath()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = @"
                Add-Type -AssemblyName System.Windows.Forms
                $dialog = New-Object System.Windows.Forms.OpenFileDialog
                $dialog.Filter = 'Аудио файлы (*.wav)|*.wav'
                $dialog.Title = 'Выберите аудио файл'
                if ($dialog.ShowDialog() -eq 'OK') { 
                    Write-Output $dialog.FileName 
                }",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string filePath = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            // Проверка результата
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Файл не выбран.");
                return null;
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл не существует: {filePath}");
                return null;
            }

            if (Path.GetExtension(filePath).ToLower() != ".wav")
            {
                Console.WriteLine("Файл должен быть в формате .wav!");
                return null;
            }

            return filePath;
        }
    }
}