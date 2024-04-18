﻿namespace Water2D
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;



    [ExecuteInEditMode]
    public class SpawnersManager : MonoBehaviour
    {
        public static SpawnersManager instance;
        [SerializeField]  bool forceToClearColorBuffers = true;
        static string _scene;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void OnLoadMethod()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpened;

        }

#endif
        void clearColorBuffers()
        {
            //if (instance == null)
              //  return;

            instance.spawnersID.Clear();
            instance.FillColorByID.Clear();
            instance.CuttOffByID.Clear();
            instance.MultiplierByID.Clear();
            instance.FresnelColorByID.Clear();
            instance.StyleByID.Clear();
            instance.LensSizeByID.Clear();
            instance.MagnitudeByID.Clear();
            instance.SpeedByID.Clear();
            //print("cleared");
        }
        void reloadAllSpawners()
        {
            Water2D_Spawner[] s = FindObjectsOfType<Water2D_Spawner>();
            for (int i = 0; i < s.Length; i++)
            {
                ChangeSpawnerValues(s[i]);
            }
        }
        void Update()
        {
            if (forceToClearColorBuffers)
            {
                forceToClearColorBuffers = false;
                clearColorBuffers();
                reloadAllSpawners();
            }

        }

        bool isCloseTo(float value1, float value2, float threshold)
        {
            float res = value1 - value2;
            if (res < 0)
                res *= -1.0f;

            if (res < threshold)
                return true;
            else
                return false;
        }

#if UNITY_EDITOR
        static void SceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            if(instance != null)
                instance.forceToClearColorBuffers = true;
        }
#endif
        static void Initialize(Color _c)
        {
            if (instance == null)
            {
                GameObject go = GameObject.Find("Water2D_SpawnersManager");
                if (go == null)
                {
                    SpawnersManager[] aux = FindObjectsOfType<SpawnersManager>();
                    for (int i = 0; i < aux.Length; i++)
                    {
                        DestroyImmediate(aux[i].gameObject);
                    }
                
                }
                else
                {
                    DestroyImmediate(go);
                }

                go = new GameObject("Water2D_SpawnersManager");
                instance = go.AddComponent<SpawnersManager>();
                go.hideFlags = HideFlags.HideInHierarchy;

                instance._arrayColors = new Color[12];
                instance._arrayCutOffStroke = new float[12];

                //instance._arrayTexture2DColors = new Texture2D[4];



                // Init Color array
                for (int i = 0; i < instance._arrayColors.Length; i++)
                {
                    instance._arrayColors[i] = _c;
                    instance._arrayCutOffStroke[i] = .3f;
                }



                if (instance.spawnersID == null)
                {
                    instance.spawnersID = new List<int>(10);
                    instance.FillColorByID = new List<Color>(10);
                    instance.CuttOffByID = new List<float>(10);
                    instance.MultiplierByID = new List<float>(10);
                    instance.FresnelColorByID = new List<Color>(10);
                    instance.StyleByID = new List<float>(10);
                    instance.LensSizeByID = new List<float>(10);
                    instance.MagnitudeByID = new List<float>(10);
                    instance.SpeedByID = new List<float>(10);
                    
                    //print("inint all");

                }
            }


        }

        public static void RegisterSpawner(Water2D_Spawner _spawner)
        {
            Initialize(_spawner.StrokeColor);

            // CHECK IF SPAWNER IS BLACK, MEANS IS UNASSIGNED COLOR //
            // SO RANDOM GENERATE AND COMPARE WITH EXIST LIST OF COLORS UNTIL BE THE ONLY ONE //
            
                if (instance.FillColorByID.Count > 0)
                {
                    int colorIsCloseTo = -1;
                    for (int i = 0; i < instance.FillColorByID.Count; i++)
                    {
                        if (instance.isCloseTo(instance.FillColorByID[i].r, _spawner.FillColor.r, 0.01f) &&
                        instance.isCloseTo(instance.FillColorByID[i].g, _spawner.FillColor.g, 0.01f) &&
                        instance.isCloseTo(instance.FillColorByID[i].b, _spawner.FillColor.b, 0.01f))
                        {
                            colorIsCloseTo = i;
                    
                        }
                    
                    }

                    if (colorIsCloseTo != -1) // Color is repeated, then generate rnd new one
                    {
                    float a = Random.value > .5 ? 1f : -1f;
                    float b = Random.value > .5 ? 1f : -1f;
                    float c = Random.value > .5 ? 1f : -1f;
                    float threshold = 0.01f;

                    _spawner.FillColor = new Color(Random.value + threshold * a, Random.value + threshold * b, Random.value + threshold * c);
                    _spawner.FillColor.a = .9f;
                    }
                    
                }
                else {
                   // _spawner.FillColor = new Color(0f, 112 / 255f, 1f, .9f);
                }
                
            //}
                        
                instance.spawnersID.Add(_spawner.GetHashCode()); // add new spawnerID
                instance.FillColorByID.Add(_spawner.FillColor);
                instance.CuttOffByID.Add(_spawner.AlphaCutOff);
                if(_spawner.Water2DType == Water2D_Spawner.EnumTypes.Refracting)
                    instance.MultiplierByID.Add(_spawner.Intensity);
                else
                    instance.MultiplierByID.Add(_spawner.AlphaStroke);

                instance.FresnelColorByID.Add(_spawner.StrokeColor);
                instance.StyleByID.Add((int)_spawner.Water2DType);
                instance.LensSizeByID.Add(_spawner.LensMag);
                instance.MagnitudeByID.Add(_spawner.Distortion);
                instance.SpeedByID.Add(_spawner.DistortionSpeed);

            //print("Init spawner : " + _spawner.name);
            _spawner.HashID = instance.spawnersID.Count - 1; //hashID used to store indice of current spawner in spawnersID list
           // print("hassh " + _spawner.HashID);
           


        }

        public static void ChangeSpawnerValues(Water2D_Spawner _spawner)
        {
            Initialize(_spawner.StrokeColor);

            //if (_spawner.HashID == -1)
            //    RegisterSpawner(_spawner);

            // Look for spawner already in scene
            int _hash = _spawner.GetHashCode();
            bool found = false;
            int idFound = -1; // _spawner.HashID;

            if(idFound == -1) { // if ID does not exist, try to find it
                for (int i = 0; i < instance.spawnersID.Count; i++)
                {
                    if (instance.spawnersID[i] == _hash)
                    {
                        found = true;
                        idFound = i;
                        break;
                    }

                }

                if (found)
                    _spawner.HashID = idFound;
                else
                    RegisterSpawner(_spawner);
            }
            

            //if does not exist, then register it
           // if (!found)
            //    RegisterSpawner(_spawner);




            // if already exists or create recently

            if (_spawner.HashID != -1)
            {
                
                       // instance.spawnersID[_spawner.HashID] = _spawner.HashID;
                instance.FillColorByID[_spawner.HashID] = _spawner.FillColor;
                instance.CuttOffByID[_spawner.HashID] = _spawner.AlphaCutOff;

                if (_spawner.Water2DType == Water2D_Spawner.EnumTypes.Refracting)
                    instance.MultiplierByID[_spawner.HashID] = _spawner.Intensity;
                else
                    instance.MultiplierByID[_spawner.HashID] = _spawner.AlphaStroke;


                instance.FresnelColorByID[_spawner.HashID] = _spawner.StrokeColor;
                instance.StyleByID[_spawner.HashID] = (int)_spawner.Water2DType;
                instance.LensSizeByID[_spawner.HashID] = (int)_spawner.Water2DType;
                instance.MagnitudeByID[_spawner.HashID] = _spawner.Distortion;
                instance.SpeedByID[_spawner.HashID] = _spawner.DistortionSpeed;


                instance.setArraysToShader(_spawner.WaterMaterial);
                instance.setFloatToShader((float)_spawner.Water2DType, _spawner.WaterMaterial);
            }
            else {

                    Debug.LogError("No set up arrays");
            }

        }

        void setFloatToShader(float f, Material mat)
        {
            mat.SetFloat("_toon", f);
        }

        public static void DeleteSpawnerValues(Water2D_Spawner _spawner)
        {
           

            // Look for spawner already in scene
            int _hash = _spawner.GetHashCode();
            bool found = false;
            int idFound = -1; // _spawner.HashID;



            if (idFound == -1)
            { // if ID does not exist, try to find it
                if (instance != null)
                {
                    for (int i = 0; i < instance.spawnersID.Count; i++)
                    {
                        if (instance.spawnersID[i] == _hash)
                        {
                            found = true;
                            idFound = i;
                            break;
                        }

                    }
                }

               

                if (found) {
                    // try to remove from arrays
                    instance.spawnersID.RemoveAt(idFound);
                    instance.FillColorByID.RemoveAt(idFound);
                    instance.CuttOffByID.RemoveAt(idFound);
                    instance.MultiplierByID.RemoveAt(idFound);
                    instance.FresnelColorByID.RemoveAt(idFound);
                    instance.StyleByID.RemoveAt(idFound);
                    instance.LensSizeByID.RemoveAt(idFound);
                    instance.MagnitudeByID.RemoveAt(idFound);
                    instance.SpeedByID.RemoveAt(idFound);

                }

            }



        }

        public List<int> spawnersID;
        public List<Color> FillColorByID;
        public List<float> CuttOffByID;
        public List<float> MultiplierByID;
        public List<Color> FresnelColorByID;
        public List<float> StyleByID;
        public List<float> LensSizeByID;
        public List<float> MagnitudeByID;
        public List<float> SpeedByID;




        void setArraysToShader(Material mat)
        {


            //print("just settings color arrays");
            mat.SetColorArray("_colors", convertToarrayOfSize(10, FillColorByID));
            mat.SetFloatArray("_cutoffs", convertToarrayOfSize(10, CuttOffByID));
            mat.SetFloatArray("_multipliers", convertToarrayOfSize(10, MultiplierByID));
            mat.SetColorArray("_fresnels", convertToarrayOfSize(10, FresnelColorByID));
            mat.SetFloatArray("_styles", convertToarrayOfSize(10, StyleByID));
            mat.SetFloatArray("_lens", convertToarrayOfSize(10, LensSizeByID));
            mat.SetFloatArray("_mags", convertToarrayOfSize(10, MagnitudeByID));
            mat.SetFloatArray("_speeds", convertToarrayOfSize(10, SpeedByID));


        }

        Color[] convertToarrayOfSize(int _size, List<Color> _list)
        {
            Color[] ret = new Color[_size];
            Color nullvalue = Color.black;

            for (int i = 0; i < _size; i++)
            {
              
                if (i < _list.Count)
                    ret[i] = _list[i];
                else
                    ret[i] = nullvalue;

            }

            return ret;
        }

        float[] convertToarrayOfSize(int _size, List<float> _list)
        {
            float[] ret = new float[_size];
            float nullvalue = -1f;

            for (int i = 0; i < _size; i++)
            {
                if (i < _list.Count)
                    ret[i] = _list[i];
                else
                    ret[i] = nullvalue;

            }

            return ret;
        }

        //Legacy
        Color[] _arrayColors;
        float[] _arrayCutOffStroke;


        //URP
        Texture2D _arrayTexture2DColors;

        Camera fresnelCamera;
        ResizeQuadEffectController quadEffect;

        public static void SetFresnelColor(Color StrokeColor)
        {
            Initialize(StrokeColor);

            if (instance.fresnelCamera == null)
            {
                instance.fresnelCamera = GameObject.Find("1-EffectCamera").GetComponent<Camera>();

            }

            Water2D_Spawner[] otherSpawners = FindObjectsOfType<Water2D_Spawner>();
            for (int i = 0; i < otherSpawners.Length; i++)
            {

                otherSpawners[i].StrokeColor = StrokeColor;
            }

            if (instance.fresnelCamera)
            {
                StrokeColor.a = 0f;
                instance.fresnelCamera.backgroundColor = StrokeColor;
            }
        }

        public static void SetSorting(int sortingID)
        {
            Initialize(Color.white);


            if (instance.quadEffect == null)
            {
                instance.quadEffect = GameObject.Find("EffectQuad").GetComponent<ResizeQuadEffectController>();

            }

            instance.quadEffect.SetSorting(sortingID);
        }



        public static MetaballParticleClass[] GetAllParticles()
        {
            if (instance == null)
                Initialize(Color.white);

            instance.fetchAllParticles();
            return instance._allparticles;
        }

        public MetaballParticleClass[] _allparticles;
        public void fetchAllParticles()
        {
            Initialize(Color.white);
            _allparticles = GameObject.FindObjectsOfType<MetaballParticleClass>();

        }


    }
}