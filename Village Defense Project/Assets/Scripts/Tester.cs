using UnityEngine;
using System.Text;
using System.Collections;

using Gem.Tools;

namespace Gem
{
    public class Tester : MonoBehaviour
    {
        ConfigFile configFile = null;

        // Use this for initialization
        void Start()
        {
            
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("Saving");
                SaveFile();
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("Loading");
                LoadFile();
            }
        }


        void SaveFile()
        {
            configFile = new ConfigFile();

            {
                ConfigFileSection chelsea = new ConfigFileSection();
                chelsea.sectionName = "Chelsea";

                ConfigVariable<string> nickName = new ConfigVariable<string>("nickName", "Cutiemoo");
                ConfigVariable<int> cutenessFactor = new ConfigVariable<int>("cutenessFactor", 9001);
                ConfigVariable<Vector3> position = new ConfigVariable<Vector3>("position", new Vector3(34.0f, 1.0f, 700.0f));

                chelsea.AddVariable(nickName);
                chelsea.AddVariable(cutenessFactor);
                chelsea.AddVariable(position);

                //chelsea.LogSection();
                configFile.AddSection(chelsea);
            }


            {
                ConfigFileSection nathan = new ConfigFileSection();
                nathan.sectionName = "Nathan";

                ConfigVariable<string> nickName = new ConfigVariable<string>("nickName", "Chips");
                ConfigVariable<int> handsomFactor = new ConfigVariable<int>("handsomFactor", -340);
                ConfigVariable<Vector3> scale = new ConfigVariable<Vector3>("scale", new Vector3(1.5f, 0.35f, 1.0f));

                nathan.AddVariable(nickName);
                nathan.AddVariable(handsomFactor);
                nathan.AddVariable(scale);

                //nathan.LogSection();
                configFile.AddSection(nathan);
            }


            configFile.Save(Application.dataPath + "\\Test.cfg");
            configFile = null;

        }

        void LoadFile()
        {
            configFile = new ConfigFile();
            configFile.Load(Application.dataPath + "\\Test.cfg");

            //{
            //    ConfigFileSection chelsea = configFile.GetSection("Chelsea");
            //    if(chelsea != null)
            //    {
            //        chelsea.LogSection();
            //    }
            //}
            //
            //{
            //    ConfigFileSection nathan = configFile.GetSection("Nathan");
            //    if(nathan != null)
            //    {
            //        nathan.LogSection();
            //    }
            //}
            configFile = null;
        }
    }
}


