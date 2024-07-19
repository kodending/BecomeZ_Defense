using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;
using DG.Tweening.Plugins;
using System.Text;

public class CSVManager : MonoBehaviour
{
    static GameObject go;

    public static CSVManager instance;

    [Tooltip("데이터 딕셔너리")]
    public Dictionary<LOCALDATALOADTYPE, GameData> m_dicData = new Dictionary<LOCALDATALOADTYPE, GameData>();

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(instance);

        LOCALDATALOADTYPE curDataType = LOCALDATALOADTYPE.COSTUMEINFO;
        while (curDataType != LOCALDATALOADTYPE._MAX_)
        {
            //게임데이터 클래스 할당
            GameData data = new GameData();
            m_dicData.Add(curDataType, data);
            m_dicData[curDataType].recordDataList = CSVReader.Read(curDataType.ToString());
            m_dicData[curDataType].recordTitleList = CSVReader.ReadTitle(curDataType.ToString());

            curDataType++;
        }
    }

    //save도 할줄 알아야한다.
    public void SaveFile(LOCALDATALOADTYPE i_eLoadType, List<Dictionary<string, object>> i_listInfo)
    {
        List<Dictionary<int, object>> TitleList = CSVManager.instance.m_dicData[i_eLoadType].recordTitleList;

        List<string[]> data = new List<string[]>();
        string[] tempData =  new string[TitleList[0].Count];

        for (int idx = 0; idx < TitleList[0].Count; idx++)
        {
            tempData[idx] = TitleList[0][idx].ToString();
        }
        
        data.Add(tempData);

        for (int idx = 0; idx < i_listInfo.Count; idx++)
        {
            string[] alphaData = new string[TitleList[0].Count];

            for (int j = 0; j < TitleList[0].Count; j++)
            {
                alphaData[j] = i_listInfo[idx][tempData[j]].ToString();
            }

            data.Add(alphaData);
        }

        string[][] output = new string[data.Count][];

        for (int idx = 0; idx < output.Length; idx++)
        {
            output[idx] = data[idx];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            sb.AppendLine(string.Join(delimiter, output[i]));
        }

        string filepath = SystemPath.GetPath();

        if (!Directory.Exists(filepath))
        {
            Directory.CreateDirectory(filepath);
        }

        string fileName = i_eLoadType.ToString() + ".csv";

        StreamWriter outStream = new StreamWriter(filepath + fileName, false, System.Text.Encoding.UTF8);
        //outStream = System.IO.File.CreateText(filepath + fileName);
        outStream.Write(sb);
        outStream.Close();
    }
}
