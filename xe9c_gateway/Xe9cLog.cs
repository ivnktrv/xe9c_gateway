using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace xe9c_gateway;

/// <summary>
/// Структура для записи событий программы в файл
/// </summary>
public readonly struct Xe9cLog
{
    private readonly string _path = "logs/"+$"{DateTime.Now}.log".Replace("/", "-").Replace(":", "-");

    public Xe9cLog()
    {
        if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
    }
    
    /// <summary>
    /// Записать событие в файл
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    public readonly void Logging(string msg, LoggingLevel level)
    {
        using StreamWriter _stream = new(_path, true);
        switch (level)
        {
            case LoggingLevel.Info:
                Console.WriteLine($"[{DateTime.Now}] [INFO] {msg}");
                _stream.WriteLine($"[{DateTime.Now}] [INFO] {msg}");
                break;
            case LoggingLevel.Warning:
                Console.WriteLine($"[{DateTime.Now}] [WARN] {msg}");
                _stream.WriteLine($"[{DateTime.Now}] [WARN] {msg}");
                break;
            case LoggingLevel.Error:
                Console.WriteLine($"[{DateTime.Now}] [ERROR] {msg}");
                _stream.WriteLine($"[{DateTime.Now}] [ERROR] {msg}");
                break;
            case LoggingLevel.Fatal:
                Console.WriteLine($"[{DateTime.Now}] [FATAL] {msg}");
                _stream.WriteLine($"[{DateTime.Now}] [FATAL] {msg}");
                break;
        }
    }

    /// <summary>
    /// Сделать "снимок" подключённых клиентов
    /// </summary>
    /// <param name="dict"></param>
    public void Frame(Dictionary<Socket, string> dict)
    {
        using StreamWriter _stream = new(_path, true);

        Console.Write($"[{DateTime.Now}] [FRAME] Подключённые клиенты: ");
        _stream.Write($"[{DateTime.Now}] [FRAME] Подключённые клиенты: ");
        foreach (var data in dict)
        {
            Console.Write($"{data.Value}, ");
            _stream.Write($"{data.Value}, ");
        }
        Console.WriteLine();
        _stream.WriteLine();
    }
}

/// <summary>
/// Серьёзность событий
/// </summary>
public enum LoggingLevel
{
    Info,
    Warning,
    Error,
    Fatal
}