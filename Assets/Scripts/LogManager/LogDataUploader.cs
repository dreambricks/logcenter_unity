using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LogDataUploader : MonoBehaviour
{
    public string outputFolder;
    private string outputPath;
    private string backupPath;
    private string datalogFolder;

    public string uploadURL;
    public int checkIntervalSeconds;

    // Start is called before the first frame update
    void Start()
    {
        datalogFolder = Path.Combine(Application.persistentDataPath, outputFolder);
        DataUploaderUtils.CheckIfDirectoryExists(datalogFolder);
        outputPath = DataUploaderUtils.EnsureCSVFilesExist(datalogFolder, "data_logs.csv");
        backupPath = DataUploaderUtils.EnsureCSVFilesExist(datalogFolder, "data_logs_backup.csv");
        StartCoroutine(Worker());
    }

    IEnumerator Worker()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkIntervalSeconds);

            // check if internet is available
            if (!DataUploaderUtils.CheckForInternetConnection())
            {
                Debug.Log("no internet available");
                continue;
            }

            if (File.Exists(outputPath))
            {
                // Lê todas as linhas do arquivo CSV
                string[] allLines = File.ReadAllLines(outputPath);
                string header = allLines[0]; // O cabeçalho é a primeira linha
                string[] lines = allLines.Skip(1).ToArray(); // Linhas de dados excluindo o cabeçalho

                // Verifica se há linhas de dados no arquivo CSV
                if (lines.Length == 0)
                {
                    Debug.Log("O arquivo CSV está vazio: " + outputPath);
                    continue;
                }

                List<string> updatedLines = new List<string>(lines);

                for (int i = 0; i < updatedLines.Count; i++)
                {
                    string line = updatedLines[i];

                    Debug.Log(string.Format("processing line '{0}' de '{1}' ", i + 1, updatedLines.Count));

                    yield return StartCoroutine(SendData(line));

                    File.AppendAllText(backupPath, line + "\n");

                    // Remova a linha do arquivo original
                    updatedLines.RemoveAt(i);

                    // Reduza o valor de i para lidar com a remoção da linha
                    i--;
                }

                // Escreva as linhas restantes de volta no arquivo CSV com o cabeçalho
                if (updatedLines.Count > 0)
                {
                    File.WriteAllLines(outputPath, new[] { header }.Concat(updatedLines).ToArray());
                }
                else
                {
                    // Se não houver linhas restantes, escreva apenas o cabeçalho
                    File.WriteAllLines(outputPath, new[] { header });
                }
            }
        }
    }

    virtual protected IEnumerator SendData(string line)
    {    

        // Crie um objeto WWWForm para armazenar o arquivo
        WWWForm form = new WWWForm();

        string[] columns = line.Split(',');

        DataLog dataLog = new DataLog();
        dataLog.timePlayed = columns[0];
        dataLog.status = columns[1];
        dataLog.project = columns[2];
        dataLog.additional = columns[3];

        form.AddField("timePlayed", dataLog.timePlayed);
        form.AddField("status", dataLog.status);
        form.AddField("project", dataLog.project);
        form.AddField("additional", dataLog.additional);

        // Crie uma requisicao UnityWebRequest para enviar o arquivo
        using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, form))
        {
            yield return www.SendWebRequest(); // Envie a requisicao

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(string.Format("Arquivo '{0}' enviado com sucesso!", line));
            }
            else
            {
                Debug.Log(string.Format("Erro ao enviar o arquivo '{0}': {1}", line, www.error));
            }
        }
    }
}
