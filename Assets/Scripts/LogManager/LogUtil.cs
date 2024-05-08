using System;
using System.IO;
using UnityEngine;

public class LogUtil : MonoBehaviour
{

    private static string logFilePath;

    public static void SaveLog(DataLog dataLog)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "data_logs");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        logFilePath = Path.Combine(folderPath, "data_logs.csv");

        // Escreva o cabeçalho do CSV se o arquivo não existir
        if (!File.Exists(logFilePath))
        {
            using (StreamWriter writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine("timePlayed,status");
            }
        }

        string formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        dataLog.timePlayed = formattedDateTime;

        // Construa a linha CSV
        string csvLine = string.Format("{0},{1},{2},{3}", dataLog.timePlayed, dataLog.status, dataLog.project, dataLog.additional);

        // Adicione a linha ao arquivo CSV
        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            writer.WriteLine(csvLine);
        }
    }

}
