using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class DataUploader : MonoBehaviour
{
    public string outputFolder;
    public string backupFolder;
    private string outputPath;
    private string backupPath;

    public string uploadURL;
    public int checkIntervalSeconds;

    // Start is called before the first frame update
    void Start()
    {
        outputPath = Path.Combine(Application.persistentDataPath, outputFolder);
        backupPath = Path.Combine(Application.persistentDataPath, backupFolder);
        DataUploaderUtils.CheckIfDirectoryExists(outputPath);
        DataUploaderUtils.CheckIfDirectoryExists(backupPath);
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

            // check if there are files to process
            // get a list of files in the output folder
            string[] fileEntries = Directory.GetFiles(outputPath);
            if (fileEntries.Length == 0)
            {
                Debug.Log("no files to process");
                continue;
            }

            foreach (string filepath in fileEntries)
            {
                string filename = Path.GetFileName(filepath);
                Debug.Log(string.Format("processing file '{0}'", filename));
                yield return StartCoroutine(SendData(filename));
            }
        }
    }

    virtual protected IEnumerator SendData(string filename)
    {

        // Crie um objeto WWWForm para armazenar o arquivo
        WWWForm form = new WWWForm();

        string fullPath = Path.Combine(outputPath, filename);

        // Carregue o arquivo binario
        byte[] fileData = System.IO.File.ReadAllBytes(fullPath);
        form.AddBinaryData("file", fileData, filename);

        // Crie uma requisicao UnityWebRequest para enviar o arquivo
        using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, form))
        {
            yield return www.SendWebRequest(); // Envie a requisicao

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(string.Format("Arquivo '{0}' enviado com sucesso!", filename));
                DataUploaderUtils.BackupFile(filename, outputPath, backupPath);
            }
            else
            {
                Debug.Log(string.Format("Erro ao enviar o arquivo '{0}': {1}", filename, www.error));
            }
        }
    }
}
