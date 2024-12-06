using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void savePlayer(){
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.txt";

        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerData LoadPlayer() {
        string path = Application.persistentDataPath + "/player.txt";

        if (File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            //c-sharp casting
            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;

        } else{
            Debug.Log("Save file not found");
            return null;
        }
    }
}