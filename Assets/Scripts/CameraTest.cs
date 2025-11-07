using AForge.Imaging.Filters;
using AForge.Video.DirectShow;
using ArcFaceSDK;
using ArcFaceSDK.Entity;
using ArcFaceSDK.SDKModels;
using ArcFaceSDK.Utils;
using ArcSoftFace.Entity;
using ArcSoftFace.Utils;
using log4net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;
using Image = System.Drawing.Image;

public class CameraTest : MonoBehaviour
{
    public RawImage rawImage;
    public TMP_Text webCamDisplayText;
    public Button btnStartVideo;

    WebCamTexture webCamTexture;

    void Start()
    {
        WebCamDevice[] cam_devices = WebCamTexture.devices;
        // for debugging purposes, prints available devices to the console
        for (int i = 0; i < cam_devices.Length; i++)
        {
            print("Webcam available: " + cam_devices[i].name);
        }

        GoWebCam01();

        InitEnginesCustom();

        btnStartVideo_Click(new object(), new EventArgs());
    }

    //CAMERA 01 SELECT
    public void GoWebCam01()
    {
        WebCamDevice[] cam_devices = WebCamTexture.devices;
        // for debugging purposes, prints available devices to the console
        for (int i = 0; i < cam_devices.Length; i++)
        {
            print("Webcam available: " + cam_devices[i].name);
        }

        webCamTexture = new WebCamTexture(cam_devices[0].name, 1280, 720, 30);
        rawImage.texture = webCamTexture;
        if (webCamTexture != null)
        {
            //webCamTexture.Play();
            Debug.Log("Web Cam Connected : " + webCamTexture.deviceName + "\n");
        }
        webCamDisplayText.text = "Camera Type: " + cam_devices[0].name.ToString();
    }

    // Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    //void Update()
    //{

    //}

    public static Image Texture2Image(Texture2D texture)
    {
        if (texture == null)
        {
            return null;
        }
        //Save the texture to the stream.
        byte[] bytes = texture.EncodeToPNG();

        //Memory stream to store the bitmap data.
        MemoryStream ms = new MemoryStream(bytes);

        //Seek the beginning of the stream.
        ms.Seek(0, SeekOrigin.Begin);

        //Create an image from a stream.
        Image bmp2 = Bitmap.FromStream(ms);

        //Close the stream, we nolonger need it.
        ms.Close();
        ms = null;

        return bmp2;
    }

    private void InitEnginesCustom()
    {
        try
        {
            webCamDisplayText.text += "    ";

            //  ȡ     ļ 
            //AppSettingsReader reader = new AppSettingsReader();
            //rgbCameraIndex = (int)reader.GetValue("RGB_CAMERA_INDEX", typeof(int));
            //irCameraIndex = (int)reader.GetValue("IR_CAMERA_INDEX", typeof(int));
            //frMatchTime = (int)reader.GetValue("FR_MATCH_TIME", typeof(int));
            //liveMatchTime = (int)reader.GetValue("LIVENESS_MATCH_TIME", typeof(int));

            AppSettingsReader reader = new AppSettingsReader();
            rgbCameraIndex = 0;
            irCameraIndex = 1;
            frMatchTime = 20;
            liveMatchTime = 20;

            int retCode = 0;
            bool isOnlineActive = true;//true(   ߼   ) or false(   ߼   )
            try
            {
                if (isOnlineActive)
                {
                    #region   ȡ   ߼         Ϣ
                    //string appId = (string)reader.GetValue("APPID", typeof(string));
                    //string sdkKey64 = (string)reader.GetValue("SDKKEY64", typeof(string));
                    //string sdkKey32 = (string)reader.GetValue("SDKKEY32", typeof(string));
                    //string activeKey64 = (string)reader.GetValue("ACTIVEKEY64", typeof(string));
                    //string activeKey32 = (string)reader.GetValue("ACTIVEKEY32", typeof(string));

                    string appId = "";
                    string sdkKey64 = "";
                    string sdkKey32 = "";
                    string activeKey64 = "";
                    string activeKey32 = "";

                    webCamDisplayText.text += "111111";

                    // ж CPUλ  
                    var is64CPU = Environment.Is64BitProcess;
                    if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(is64CPU ? sdkKey64 : sdkKey32) || string.IsNullOrWhiteSpace(is64CPU ? activeKey64 : activeKey32))
                    {
                        Debug.LogError(string.Format("    App.config     ļ         APP_ID  SDKKEY{0}  ACTIVEKEY{0}!", is64CPU ? "64" : "32"));
                        //MessageBox.Show(string.Format("    App.config     ļ         APP_ID  SDKKEY{0}  ACTIVEKEY{0}!", is64CPU ? "64" : "32"));

                        //System.Environment.Exit(0);
                        Quit();
                    }
                    #endregion

                    webCamDisplayText.text += "׼      ";

                    //   ߼               ִ   1.    ȷ ϴӹ      ص sdk   ѷŵ   Ӧ  bin У 2.  ǰѡ   CPUΪx86    x64
                    retCode = imageEngine.ASFOnlineActivation(appId, is64CPU ? sdkKey64 : sdkKey32, is64CPU ? activeKey64 : activeKey32);

                    webCamDisplayText.text += "       ";
                }
                else
                {
                    #region   ȡ   ߼         Ϣ
                    string offlineActiveFilePath = (string)reader.GetValue("OfflineActiveFilePath", typeof(string));
                    if (string.IsNullOrWhiteSpace(offlineActiveFilePath) || !File.Exists(offlineActiveFilePath))
                    {
                        string deviceInfo;
                        retCode = imageEngine.ASFGetActiveDeviceInfo(out deviceInfo);
                        if (retCode != 0)
                        {
                            Debug.LogError("  ȡ 豸  Ϣʧ ܣ       :" + retCode);
                            //MessageBox.Show("  ȡ 豸  Ϣʧ ܣ       :" + retCode);
                        }
                        else
                        {
                            File.WriteAllText("ActiveDeviceInfo.txt", deviceInfo);
                            Debug.LogError("  ȡ 豸  Ϣ ɹ    ѱ  浽   и Ŀ¼ActiveDeviceInfo.txt ļ      ڹ   ִ     ߼             ɵ       Ȩ ļ ·    App.config     ú           ");
                            //MessageBox.Show("  ȡ 豸  Ϣ ɹ    ѱ  浽   и Ŀ¼ActiveDeviceInfo.txt ļ      ڹ   ִ     ߼             ɵ       Ȩ ļ ·    App.config     ú           ");
                        }
                        //System.Environment.Exit(0);
                        Quit();
                    }
                    #endregion
                    //   ߼   
                    retCode = imageEngine.ASFOfflineActivation(offlineActiveFilePath);
                }
                if (retCode != 0 && retCode != 90114)
                {
                    Debug.LogError("    SDKʧ  ,      :" + retCode);
                    //MessageBox.Show("    SDKʧ  ,      :" + retCode);
                    //System.Environment.Exit(0);
                    Quit();
                }

                webCamDisplayText.text += retCode.ToString();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(" ޷      DLL"))
                {
                    Debug.LogError(" 뽫SDK   DLL    bin  Ӧ  x86  x64 µ  ļ     !");
                    //MessageBox.Show(" 뽫SDK   DLL    bin  Ӧ  x86  x64 µ  ļ     !");
                }
                else
                {
                    Debug.LogError("    SDKʧ  ,   ȼ            SDK  ƽ̨   汾 Ƿ   ȷ!");
                    //MessageBox.Show("    SDKʧ  ,   ȼ            SDK  ƽ̨   汾 Ƿ   ȷ!");
                }
                //System.Environment.Exit(0);
                Quit();
            }

            //  ʼ      
            DetectionMode detectMode = DetectionMode.ASF_DETECT_MODE_IMAGE;
            //Videoģʽ ¼       ĽǶ     ֵ
            ASF_OrientPriority videoDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
            //Imageģʽ ¼       ĽǶ     ֵ
            ASF_OrientPriority imageDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
            //     Ҫ            
            int detectFaceMaxNum = 6;
            //     ʼ  ʱ  Ҫ  ʼ   ļ ⹦     
            int combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_AGE | FaceEngineMask.ASF_GENDER | FaceEngineMask.ASF_FACE3DANGLE | FaceEngineMask.ASF_IMAGEQUALITY | FaceEngineMask.ASF_MASKDETECT;
            //  ʼ     棬    ֵΪ0          ֵ  ο http://ai.arcsoft.com.cn/bbs/forum.php?mod=viewthread&tid=19&_dsign=dbad527e
            retCode = imageEngine.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceMaxNum, combinedMask);
            Console.WriteLine("InitEngine Result:" + retCode);
            AppendText((retCode == 0) ? "ͼƬ     ʼ   ɹ !" : string.Format("ͼƬ     ʼ  ʧ  !      Ϊ:{0}", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }

            //  ʼ    Ƶģʽ             
            DetectionMode detectModeVideo = DetectionMode.ASF_DETECT_MODE_VIDEO;
            int combinedMaskVideo = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_FACELANDMARK;
            retCode = videoEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceMaxNum, combinedMaskVideo);
            AppendText((retCode == 0) ? "  Ƶ     ʼ   ɹ !" : string.Format("  Ƶ     ʼ  ʧ  !      Ϊ:{0}", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }

            //RGB  Ƶר  FR    
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_LIVENESS | FaceEngineMask.ASF_MASKDETECT;
            retCode = videoRGBImageEngine.ASFInitEngine(detectMode, videoDetectFaceOrientPriority, detectFaceMaxNum, combinedMask);
            AppendText((retCode == 0) ? "RGB         ʼ   ɹ !" : string.Format("RGB         ʼ  ʧ  !      Ϊ:{0}", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }
            //   û     ֵ
            videoRGBImageEngine.ASFSetLivenessParam(thresholdRgb);

            //IR  Ƶר  FR    
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_IR_LIVENESS;
            retCode = videoIRImageEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceMaxNum, combinedMask);
            AppendText((retCode == 0) ? "IR         ʼ   ɹ !\r\n" : string.Format("IR         ʼ  ʧ  !      Ϊ:{0}\r\n", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }
            //   û     ֵ
            videoIRImageEngine.ASFSetLivenessParam(thresholdRgb, thresholdIr);

            initVideo();
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
            Debug.LogError("     ʼ   쳣,    App.config   ޸   ־    ,      ־    ԭ  !");
            //MessageBox.Show("     ʼ   쳣,    App.config   ޸   ־    ,      ־    ԭ  !");

            Quit();
            //System.Environment.Exit(0);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region         
    /// <summary>
    /// ͼ           
    /// </summary>
    private FaceEngine imageEngine = new FaceEngine();

    /// <summary>
    ///      Ҳ ͼƬ·  
    /// </summary>
    private string image1Path;

    /// <summary>
    /// ͼƬ    С    
    /// </summary>
    private long maxSize = 1024 * 1024 * 2;

    /// <summary>
    ///      
    /// </summary>
    private int maxWidth = 1536;

    /// <summary>
    ///    ߶ 
    /// </summary>
    private int maxHeight = 1536;

    /// <summary>
    ///  ȶ     ͼƬ        
    /// </summary>
    private List<FaceFeature> rightImageFeatureList = new List<FaceFeature>();

    /// <summary>
    ///     Ա ͼƬ   б 
    /// </summary>
    private List<string> imagePathList = new List<string>();

    /// <summary>
    ///                б 
    /// </summary>
    private List<FaceFeature> leftImageFeatureList = new List<FaceFeature>();

    /// <summary>
    ///      ȶ   ֵ
    /// </summary>
    private float threshold = 0.8f;

    /// <summary>
    ///    ⣨IR        ֵ
    /// </summary>
    private float thresholdIr = 0.7f;

    /// <summary>
    ///  ɼ  ⣨RGB        ֵ
    /// </summary>
    private float thresholdRgb = 0.5f;

    /// <summary>
    /// ͼ      ע    ֵ
    /// </summary>
    private float thresholdImgRegister = 0.63f;

    /// <summary>
    /// ͼ      ʶ         ֵ
    /// </summary>
    private float thresholdImgMask = 0.29f;

    /// <summary>
    /// ͼ      ʶ  δ        ֵ
    /// </summary>
    private float thresholdImgNoMask = 0.49f;
    /// <summary>
    ///  ȶ ģ  
    /// </summary>
    private ASF_CompareModel compareModel = ASF_CompareModel.ASF_ID_PHOTO;
    /// <summary>
    ///    ڱ   Ƿ   Ҫ    ȶԽ  
    /// </summary>
    private bool isCompare = false;

    #region   Ƶģʽ     
    /// <summary>
    ///   Ƶ       
    /// </summary>
    private FaceEngine videoEngine = new FaceEngine();

    /// <summary>
    /// RGB  Ƶ       
    /// </summary>
    private FaceEngine videoRGBImageEngine = new FaceEngine();

    /// <summary>
    /// IR  Ƶ       
    /// </summary>
    private FaceEngine videoIRImageEngine = new FaceEngine();

    /// <summary>
    ///   Ƶ     豸  Ϣ
    /// </summary>
    private FilterInfoCollection filterInfoCollection;

    /// <summary>
    /// RGB    ͷ 豸
    /// </summary>
    private VideoCaptureDevice rgbDeviceVideo;

    /// <summary>
    /// IR    ͷ 豸
    /// </summary>
    private VideoCaptureDevice irDeviceVideo;

    /// <summary>
    ///  Ƿ   ˫Ŀ    
    /// </summary>
    private bool isDoubleShot = false;

    /// <summary>
    /// RGB     ͷ    
    /// </summary>
    private int rgbCameraIndex = 0;

    /// <summary>
    /// IR     ͷ    
    /// </summary>
    private int irCameraIndex = 0;

    /// <summary>
    ///   Ա  ͼƬѡ         
    /// </summary>
    private object chooseImgLocker = new object();

    /// <summary>
    /// RGB  Ƶ֡ͼ  ʹ    
    /// </summary>
    private object rgbVideoImageLocker = new object();
    /// <summary>
    /// IR  Ƶ֡ͼ  ʹ    
    /// </summary>
    private object irVideoImageLocker = new object();
    /// <summary>
    /// RGB  Ƶ֡ͼ  
    /// </summary>
    private Bitmap rgbVideoBitmap = null;
    /// <summary>
    /// IR  Ƶ֡ͼ  
    /// </summary>
    private Bitmap irVideoBitmap = null;
    /// <summary>
    /// RGB     ͷ  Ƶ    ׷ ټ    
    /// </summary>
    private DictionaryUnit<int, FaceTrackUnit> trackRGBUnitDict = new DictionaryUnit<int, FaceTrackUnit>();

    /// <summary>
    /// RGB            Դ    ֵ 
    /// </summary>
    private DictionaryUnit<int, int> rgbFeatureTryDict = new DictionaryUnit<int, int>();

    /// <summary>
    /// RGB      Ⳣ Դ    ֵ 
    /// </summary>
    private DictionaryUnit<int, int> rgbLivenessTryDict = new DictionaryUnit<int, int>();

    /// <summary>
    /// IR   Ƶ       ׷ ټ    
    /// </summary>
    private FaceTrackUnit trackIRUnit = new FaceTrackUnit();

    /// <summary>
    /// VideoPlayer        
    /// </summary>
    private Font font = new Font(FontFamily.GenericSerif, 10f, FontStyle.Bold);

    /// <summary>
    ///   ɫ    
    /// </summary>
    private SolidBrush redBrush = new SolidBrush(Color.Red);

    /// <summary>
    ///   ɫ    
    /// </summary>
    private SolidBrush greenBrush = new SolidBrush(Color.Green);

    /// <summary>
    ///  ر FR ߳̿   
    /// </summary>
    private bool exitVideoRGBFR = false;

    /// <summary>
    ///  رջ    ߳̿   
    /// </summary>
    private bool exitVideoRGBLiveness = false;
    /// <summary>
    ///  ر IR     FR ߳  ߳̿   
    /// </summary>
    private bool exitVideoIRFRLiveness = false;
    /// <summary>
    /// FRʧ     Դ   
    /// </summary>
    private int frMatchTime = 30;

    /// <summary>
    ///       ʧ     Դ   
    /// </summary>
    private int liveMatchTime = 30;
    #endregion
    #endregion

    #region   ʼ  
    public void FaceForm()
    {
        //InitializeComponent();
        //CheckForIllegalCrossThreadCalls = false;
        //  ʼ      
        InitEngines();
        //        ͷͼ 񴰿 
        //rgbVideoSource.Hide();
        //irVideoSource.Hide();
    }

    /// <summary>
    ///   ʼ      
    /// </summary>
    private void InitEngines()
    {
        try
        {
            //  ȡ     ļ 
            AppSettingsReader reader = new AppSettingsReader();
            rgbCameraIndex = (int)reader.GetValue("RGB_CAMERA_INDEX", typeof(int));
            irCameraIndex = (int)reader.GetValue("IR_CAMERA_INDEX", typeof(int));
            frMatchTime = (int)reader.GetValue("FR_MATCH_TIME", typeof(int));
            liveMatchTime = (int)reader.GetValue("LIVENESS_MATCH_TIME", typeof(int));

            int retCode = 0;
            bool isOnlineActive = true;//true(   ߼   ) or false(   ߼   )
            try
            {
                if (isOnlineActive)
                {
                    #region   ȡ   ߼         Ϣ
                    string appId = (string)reader.GetValue("APPID", typeof(string));
                    string sdkKey64 = (string)reader.GetValue("SDKKEY64", typeof(string));
                    string sdkKey32 = (string)reader.GetValue("SDKKEY32", typeof(string));
                    string activeKey64 = (string)reader.GetValue("ACTIVEKEY64", typeof(string));
                    string activeKey32 = (string)reader.GetValue("ACTIVEKEY32", typeof(string));
                    // ж CPUλ  
                    var is64CPU = Environment.Is64BitProcess;
                    if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(is64CPU ? sdkKey64 : sdkKey32) || string.IsNullOrWhiteSpace(is64CPU ? activeKey64 : activeKey32))
                    {
                        //MessageBox.Show(string.Format("    App.config     ļ         APP_ID  SDKKEY{0}  ACTIVEKEY{0}!", is64CPU ? "64" : "32"));
                        Debug.LogError(string.Format("    App.config     ļ         APP_ID  SDKKEY{0}  ACTIVEKEY{0}!", is64CPU ? "64" : "32"));
                        System.Environment.Exit(0);
                    }
                    #endregion
                    //   ߼               ִ   1.    ȷ ϴӹ      ص sdk   ѷŵ   Ӧ  bin У 2.  ǰѡ   CPUΪx86    x64
                    retCode = imageEngine.ASFOnlineActivation(appId, is64CPU ? sdkKey64 : sdkKey32, is64CPU ? activeKey64 : activeKey32);
                }
                else
                {
                    #region   ȡ   ߼         Ϣ
                    string offlineActiveFilePath = (string)reader.GetValue("OfflineActiveFilePath", typeof(string));
                    if (string.IsNullOrWhiteSpace(offlineActiveFilePath) || !File.Exists(offlineActiveFilePath))
                    {
                        string deviceInfo;
                        retCode = imageEngine.ASFGetActiveDeviceInfo(out deviceInfo);
                        if (retCode != 0)
                        {
                            //MessageBox.Show("  ȡ 豸  Ϣʧ ܣ       :" + retCode);
                            Debug.LogError("  ȡ 豸  Ϣʧ ܣ       :" + retCode);
                        }
                        else
                        {
                            File.WriteAllText("ActiveDeviceInfo.txt", deviceInfo);
                            //MessageBox.Show("  ȡ 豸  Ϣ ɹ    ѱ  浽   и Ŀ¼ActiveDeviceInfo.txt ļ      ڹ   ִ     ߼             ɵ       Ȩ ļ ·    App.config     ú           ");
                            Debug.LogError("  ȡ 豸  Ϣ ɹ    ѱ  浽   и Ŀ¼ActiveDeviceInfo.txt ļ      ڹ   ִ     ߼             ɵ       Ȩ ļ ·    App.config     ú           ");
                        }
                        System.Environment.Exit(0);
                    }
                    #endregion
                    //   ߼   
                    retCode = imageEngine.ASFOfflineActivation(offlineActiveFilePath);
                }
                if (retCode != 0 && retCode != 90114)
                {
                    //MessageBox.Show("    SDKʧ  ,      :" + retCode);
                    Debug.LogError("    SDKʧ  ,      :" + retCode);
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(" ޷      DLL"))
                {
                    //MessageBox.Show(" 뽫SDK   DLL    bin  Ӧ  x86  x64 µ  ļ     !");
                    Debug.LogError(" 뽫SDK   DLL    bin  Ӧ  x86  x64 µ  ļ     !");
                }
                else
                {
                    //MessageBox.Show("    SDKʧ  ,   ȼ            SDK  ƽ̨   汾 Ƿ   ȷ!");
                    Debug.LogError("    SDKʧ  ,   ȼ            SDK  ƽ̨   汾 Ƿ   ȷ!");
                }
                System.Environment.Exit(0);
            }

            //  ʼ      
            DetectionMode detectMode = DetectionMode.ASF_DETECT_MODE_IMAGE;
            //Videoģʽ ¼       ĽǶ     ֵ
            ASF_OrientPriority videoDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
            //Imageģʽ ¼       ĽǶ     ֵ
            ASF_OrientPriority imageDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
            //     Ҫ            
            int detectFaceMaxNum = 6;
            //     ʼ  ʱ  Ҫ  ʼ   ļ ⹦     
            int combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_AGE | FaceEngineMask.ASF_GENDER | FaceEngineMask.ASF_FACE3DANGLE | FaceEngineMask.ASF_IMAGEQUALITY | FaceEngineMask.ASF_MASKDETECT;
            //  ʼ     棬    ֵΪ0          ֵ  ο http://ai.arcsoft.com.cn/bbs/forum.php?mod=viewthread&tid=19&_dsign=dbad527e
            retCode = imageEngine.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceMaxNum, combinedMask);
            Console.WriteLine("InitEngine Result:" + retCode);
            AppendText((retCode == 0) ? "ͼƬ     ʼ   ɹ !" : string.Format("ͼƬ     ʼ  ʧ  !      Ϊ:{0}", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }

            //  ʼ    Ƶģʽ             
            DetectionMode detectModeVideo = DetectionMode.ASF_DETECT_MODE_VIDEO;
            int combinedMaskVideo = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_FACELANDMARK;
            retCode = videoEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceMaxNum, combinedMaskVideo);
            AppendText((retCode == 0) ? "  Ƶ     ʼ   ɹ !" : string.Format("  Ƶ     ʼ  ʧ  !      Ϊ:{0}", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }

            //RGB  Ƶר  FR    
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_LIVENESS | FaceEngineMask.ASF_MASKDETECT;
            retCode = videoRGBImageEngine.ASFInitEngine(detectMode, videoDetectFaceOrientPriority, detectFaceMaxNum, combinedMask);
            AppendText((retCode == 0) ? "RGB         ʼ   ɹ !" : string.Format("RGB         ʼ  ʧ  !      Ϊ:{0}", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }
            //   û     ֵ
            videoRGBImageEngine.ASFSetLivenessParam(thresholdRgb);

            //IR  Ƶר  FR    
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_IR_LIVENESS;
            retCode = videoIRImageEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceMaxNum, combinedMask);
            AppendText((retCode == 0) ? "IR         ʼ   ɹ !\r\n" : string.Format("IR         ʼ  ʧ  !      Ϊ:{0}\r\n", retCode));
            if (retCode != 0)
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }
            //   û     ֵ
            videoIRImageEngine.ASFSetLivenessParam(thresholdRgb, thresholdIr);

            initVideo();
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
            //MessageBox.Show("     ʼ   쳣,    App.config   ޸   ־    ,      ־    ԭ  !");
            Debug.LogError("     ʼ   쳣,    App.config   ޸   ־    ,      ־    ԭ  !");
            System.Environment.Exit(0);
        }
    }

    /// <summary>
    ///     ͷ  ʼ  
    /// </summary>
    private void initVideo()
    {
        try
        {
            //filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //   û п       ͷ            ͷ    ť   ã     ʹ    
            btnStartVideo.enabled = WebCamTexture.devices.Length != 0;
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region ע        ť ¼         
    /// <summary>
    ///       ͼƬѡ  ť ¼ 
    /// </summary>
    private void ChooseMultiImg(object sender, EventArgs e)
    {
        try
        {
            lock (chooseImgLocker)
            {
                //OpenFileDialog openFileDialog = new OpenFileDialog();
                //openFileDialog.Title = "ѡ  ͼƬ";
                //openFileDialog.Filter = "ͼƬ ļ |*.bmp;*.jpg;*.jpeg;*.png";
                //openFileDialog.Multiselect = true;
                //openFileDialog.FileName = string.Empty;
                //imageList.Refresh();
                //if (!openFileDialog.ShowDialog().Equals(DialogResult.OK))
                {
                    return;
                }
                List<string> imagePathListTemp = new List<string>();
                var numStart = imagePathList.Count;
                //int isGoodImage = 0;

                //        Լ   ȡ        
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                {
                    //  ֹ     ť
                    //Invoke(new Action(delegate
                    //{
                    //    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn, btnStartVideo);
                    //}));

                    //    ͼƬ·      ʾ
                    //string[] fileNames = openFileDialog.FileNames;
                    //for (int i = 0; i < fileNames.Length; i++)
                    //{
                    //    //ͼƬ  ʽ ж 
                    //    if (CheckImage(fileNames[i]))
                    //    {
                    //        imagePathListTemp.Add(fileNames[i]);
                    //    }
                    //}
                    //       ͼ  
                    for (int i = 0; i < imagePathListTemp.Count; i++)
                    {
                        Image image = ImageUtil.ReadFromFile(imagePathListTemp[i]);
                        //У  ͼƬ   
                        CheckImageWidthAndHeight(ref image);
                        if (image == null)
                        {
                            continue;
                        }
                        //    ͼ   ȣ   Ҫ   Ϊ4 ı   
                        if (image.Width % 4 != 0)
                        {
                            image = ImageUtil.ScaleImage(image, image.Width - (image.Width % 4), image.Height);
                        }
                        //  ȡ     ж 
                        string featureResult = string.Empty;
                        bool isMask;
                        int retCode;
                        SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                        FaceFeature feature = FaceUtil.ExtractFeature(imageEngine, image, thresholdImgRegister, thresholdImgMask, ASF_RegisterOrNot.ASF_REGISTER, out singleFaceInfo, out isMask, ref featureResult, out retCode);
                        if (!string.IsNullOrEmpty(featureResult))
                        {
                            //this.Invoke(new Action(delegate
                            //{
                            //    AppendText(featureResult);
                            //}));
                            if (image != null)
                            {
                                image.Dispose();
                            }
                            continue;
                        }
                        //       
                        MultiFaceInfo multiFaceInfo;
                        retCode = imageEngine.ASFDetectFacesEx(image, out multiFaceInfo);
                        // жϼ    
                        if (retCode == 0 && multiFaceInfo.faceNum > 0)
                        {
                            //      ʱ  Ĭ ϲü  һ      
                            imagePathList.Add(imagePathListTemp[i]);
                            MRECT rect = multiFaceInfo.faceRects[0];
                            image = ImageUtil.CutImage(image, rect.left, rect.top, rect.right, rect.bottom);
                        }
                        else
                        {
                            //this.Invoke(new Action(delegate
                            //{
                            //    AppendText("δ  ⵽    ");
                            //}));
                            if (image != null)
                            {
                                image.Dispose();
                            }
                            continue;
                        }

                        //  ʾ    
                        //this.Invoke(new Action(delegate
                        //{
                        //    if (image == null)
                        //    {
                        //        image = ImageUtil.ReadFromFile(imagePathListTemp[i]);
                        //        //У  ͼƬ   
                        //        CheckImageWidthAndHeight(ref image);
                        //    }
                        //    //imageLists.Images.Add(imagePathListTemp[i], image);
                        //    //imageList.Items.Add((numStart + isGoodImage) + "  ", imagePathListTemp[i]);
                        //    //imageList.Refresh();
                        //    AppendText(string.Format("    ȡ{0}          ֵ  [left:{1},right:{2},top:{3},bottom:{4},orient:{5},mask:{6}]", (numStart + isGoodImage), singleFaceInfo.faceRect.left, singleFaceInfo.faceRect.right, singleFaceInfo.faceRect.top, singleFaceInfo.faceRect.bottom, singleFaceInfo.faceOrient, isMask ? "mask" : "no mask"));
                        //    leftImageFeatureList.Add(feature);
                        //    isGoodImage++;
                        //    if (image != null)
                        //    {
                        //        image.Dispose();
                        //    }
                        //}));
                    }
                    //        ť
                    //Invoke(new Action(delegate
                    //{
                    //    //ControlsEnable(true, chooseMultiImgBtn, btnClearFaceList, btnStartVideo);
                    //    //ControlsEnable(("        ͷ".Equals(btnStartVideo.Text)), chooseImgBtn, matchBtn);
                    //}));
                }));

            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region         ⰴť ¼ 
    /// <summary>
    ///           ¼ 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnClearFaceList_Click(object sender, EventArgs e)
    {
        try
        {
            //       
            //imageLists.Images.Clear();
            //imageList.Items.Clear();
            leftImageFeatureList.Clear();
            imagePathList.Clear();
            //imageList.Refresh();
            GC.Collect();
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region ѡ  ʶ  ͼ  ť ¼ 
    /// <summary>
    ///   ѡ  ʶ  ͼƬ    ť ¼ 
    /// </summary>
    private void ChooseImg(object sender, EventArgs e)
    {
        try
        {
            //lblCompareInfo.Text = string.Empty;
            // ж      Ƿ  ʼ   ɹ 
            if (!imageEngine.GetEngineStatus())
            {
                //      ع  ܰ ť
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                //MessageBox.Show("   ȳ ʼ      !");
                Debug.LogError("   ȳ ʼ      !");
                return;
            }
            //ѡ  ͼƬ
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Title = "ѡ  ͼƬ";
            //openFileDialog.Filter = "ͼƬ ļ |*.bmp;*.jpg;*.jpeg;*.png";
            //openFileDialog.Multiselect = false;
            //openFileDialog.FileName = string.Empty;
            //if (openFileDialog.ShowDialog().Equals(DialogResult.OK))
            {
                //image1Path = openFileDialog.FileName;
                //   ͼƬ  ʽ
                //if (!CheckImage(image1Path))
                {
                    return;
                }
                DateTime detectStartTime = DateTime.Now;
                AppendText(string.Format("--------------  ʼ  ⣬ʱ  :{0}--------------", detectStartTime.ToString("yyyy-MM-dd HH:mm:ss:ms")));

                //  ȡ ļ    ܾ      ͼƬ
                FileInfo fileInfo = new FileInfo(image1Path);
                if (fileInfo.Length > maxSize)
                {
                    //MessageBox.Show("ͼ   ļ    Ϊ2MB    ѹ     ٵ   !");
                    AppendText(string.Format("--------------        ʱ  :{0}--------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                    AppendText("");
                    return;
                }

                Image srcImage = ImageUtil.ReadFromFile(image1Path);
                //У  ͼƬ   
                CheckImageWidthAndHeight(ref srcImage);
                if (srcImage == null)
                {
                    //MessageBox.Show("ͼ     ݻ ȡʧ ܣ    Ժ     !");
                    AppendText(string.Format("--------------        ʱ  :{0}--------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                    AppendText("");
                    return;
                }
                //    ͼ   ȣ   Ҫ   Ϊ4 ı   
                if (srcImage.Width % 4 != 0)
                {
                    srcImage = ImageUtil.ScaleImage(srcImage, srcImage.Width - (srcImage.Width % 4), srcImage.Height);
                }
                //       
                MultiFaceInfo multiFaceInfo;
                int retCode = imageEngine.ASFDetectFacesEx(srcImage, out multiFaceInfo);
                if (retCode != 0)
                {
                    //MessageBox.Show("ͼ         ʧ ܣ    Ժ     !");
                    AppendText(string.Format("--------------        ʱ  :{0}--------------\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                    AppendText("");
                    return;
                }
                rightImageFeatureList.Clear();
                if (multiFaceInfo.faceNum < 1)
                {
                    //srcImage = ImageUtil.ScaleImage(srcImage, picImageCompare.Width, picImageCompare.Height);
                    //picImageCompare.Image = srcImage;
                    AppendText(string.Format("{0} - δ        !\r\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                    AppendText(string.Format("--------------        ʱ  :{0}--------------\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                    AppendText("");
                    return;
                }

                //      
                int retCode_Age = -1;
                AgeInfo ageInfo = FaceUtil.AgeEstimation(imageEngine, srcImage, multiFaceInfo, out retCode_Age);
                // Ա   
                int retCode_Gender = -1;
                GenderInfo genderInfo = FaceUtil.GenderEstimation(imageEngine, srcImage, multiFaceInfo, out retCode_Gender);
                //3DAngle   
                int retCode_3DAngle = -1;
                Face3DAngle face3DAngleInfo = FaceUtil.Face3DAngleDetection(imageEngine, srcImage, multiFaceInfo, out retCode_3DAngle);

                AppendText(string.Format("{0} -         :{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), multiFaceInfo.faceNum));

                MRECT[] mrectTemp = new MRECT[multiFaceInfo.faceNum];
                int[] ageTemp = new int[multiFaceInfo.faceNum];
                int[] genderTemp = new int[multiFaceInfo.faceNum];
                bool[] maskTemp = new bool[multiFaceInfo.faceNum];
                SingleFaceInfo singleFaceInfo;

                //  ǳ   ⵽      
                for (int i = 0; i < multiFaceInfo.faceNum; i++)
                {
                    MRECT rect = multiFaceInfo.faceRects[i];
                    int orient = multiFaceInfo.faceOrients[i];
                    int age = 0;
                    //      
                    if (retCode_Age != 0)
                    {
                        AppendText(string.Format("      ʧ ܣ     {0}!", retCode_Age));
                    }
                    else
                    {
                        age = ageInfo.ageArray[i];
                    }
                    // Ա   
                    int gender = -1;
                    if (retCode_Gender != 0)
                    {
                        AppendText(string.Format(" Ա   ʧ ܣ     {0}!", retCode_Gender));
                    }
                    else
                    {
                        gender = genderInfo.genderArray[i];
                    }
                    //3DAngle   
                    float roll = 0f;
                    float pitch = 0f;
                    float yaw = 0f;
                    if (retCode_3DAngle != 0)
                    {
                        AppendText(string.Format("3DAngle   ʧ ܣ     {0}!", retCode_3DAngle));
                    }
                    else
                    {
                        //rollΪ    ǣ pitchΪ     ǣ yawΪƫ    
                        roll = face3DAngleInfo.roll[i];
                        pitch = face3DAngleInfo.pitch[i];
                        yaw = face3DAngleInfo.yaw[i];
                    }
                    //   ּ     ȡ        
                    bool isMask;
                    string faceFeatureStr = string.Empty;
                    FaceFeature tempFaceFeature = FaceUtil.ExtractFeature(imageEngine, srcImage, thresholdImgNoMask, thresholdImgMask, ASF_RegisterOrNot.ASF_RECOGNITION, out singleFaceInfo, out isMask, ref faceFeatureStr, out retCode, i);
                    if (retCode.Equals(0) && string.IsNullOrEmpty(faceFeatureStr))
                    {
                        rightImageFeatureList.Add(tempFaceFeature);
                    }
                    else
                    {
                        AppendText(faceFeatureStr);
                    }
                    maskTemp[i] = isMask;
                    mrectTemp[i] = rect;
                    ageTemp[i] = age;
                    genderTemp[i] = gender;
                    AppendText(string.Format("{0} -   {1}        :[left:{2},top:{3},right:{4},bottom:{5},\r\norient:{6},roll:{7},pitch:{8},yaw:{9},wearGlasses:{10},\r\nleftEyeClosed:{11},rightEyeClosed:{12},faceShelter:{13}],Age:{14},Gender:{15},Mask:{16}\r\n--------------------------------------------------------",
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), i, rect.left, rect.top, rect.right, rect.bottom, orient, roll, pitch, yaw, multiFaceInfo.wearGlasses[i].ToString("f2"), multiFaceInfo.leftEyeClosed[i], multiFaceInfo.rightEyeClosed[i],
                        multiFaceInfo.faceShelter[i], age, (gender >= 0 ? gender.ToString() : ""), (isMask ? "mask" : "no mask")));
                }
                AppendText(string.Format("--------------        ʱ  :{0}--------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                AppendText("");

                //    ϴε ƥ    
                for (int i = 0; i < leftImageFeatureList.Count; i++)
                {
                    //imageList.Items[i].Text = string.Format("{0}  ", i);
                }
                //  ȡ   ű   
                //float scaleRate = ImageUtil.GetWidthAndHeight(srcImage.Width, srcImage.Height, picImageCompare.Width, picImageCompare.Height);
                //    ͼƬ
                //srcImage = ImageUtil.ScaleImage(srcImage, picImageCompare.Width, picImageCompare.Height);
                //  ӱ  
                //srcImage = ImageUtil.MarkRectAndString(srcImage, mrectTemp, ageTemp, genderTemp, maskTemp, picImageCompare.Width, scaleRate, multiFaceInfo.faceNum);

                //  ʾ  Ǻ  ͼ  
                //picImageCompare.Image = srcImage;
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region   ʼƥ 䰴ť ¼ 
    /// <summary>
    /// ƥ   ¼ 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void matchBtn_Click(object sender, EventArgs e)
    {
        try
        {
            if (leftImageFeatureList.Count == 0)
            {
                //MessageBox.Show("  ע      !", "  ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.LogError("  ע      !");
                return;
            }

            if (rightImageFeatureList == null || rightImageFeatureList.Count <= 0)
            {
                //if (picImageCompare.Image == null)
                //{
                //    MessageBox.Show("  ѡ  ʶ  ͼ!", "  ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                //else
                //{
                //    MessageBox.Show(" ȶ ʧ ܣ ʶ  ͼδ  ȡ      ֵ!", "  ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                return;
            }
            //    Ѿ     ƥ  ȶԣ  ڿ     Ƶ  ʱ  Ҫ    ȶԽ  
            isCompare = true;
            AppendText(string.Format("--------------  ʼ ȶԣ ʱ  :{0}--------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
            for (int faceIndex = 0; faceIndex < rightImageFeatureList.Count; faceIndex++)
            {
                float compareSimilarity = 0f;
                int compareNum = 0;
                FaceFeature tempFaceFeature = rightImageFeatureList[faceIndex];
                if (tempFaceFeature.featureSize <= 0)
                {
                    AppendText(string.Format(" ȶ       {0}            ȡʧ  ", faceIndex));
                    continue;
                }
                AppendText(string.Format("------  ʼƥ  ȶ       {0}      ------", faceIndex));
                for (int i = 0; i < leftImageFeatureList.Count; i++)
                {
                    FaceFeature feature = leftImageFeatureList[i];
                    float similarity = 0f;
                    imageEngine.ASFFaceFeatureCompare(tempFaceFeature, feature, out similarity, compareModel);
                    //     쳣ֵ    
                    if (similarity.ToString().IndexOf("E") > -1)
                    {
                        similarity = 0f;
                    }
                    AppendText(string.Format("        {0} űȶԽ  :{1}", i, similarity));
                    if (similarity > compareSimilarity)
                    {
                        compareSimilarity = similarity;
                        compareNum = i;
                    }
                }
                if (compareSimilarity > 0)
                {
                    AppendText(string.Format("-----------------------------------\r\nƥ    :{0}  , ȶԽ  :{1}", compareNum, compareSimilarity));
                }
            }
            AppendText(string.Format("-------------- ȶԽ     ʱ  :{0}--------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region   Ƶ      (<    ͷ  ť    ¼       ͷPaint ¼        ȶԡ     ͷ        ¼ >)

    //AForge.Controls.VideoSourcePlayer rgbVideoSource = new AForge.Controls.VideoSourcePlayer();
    //AForge.Controls.VideoSourcePlayer irVideoSource = new AForge.Controls.VideoSourcePlayer();

    /// <summary>
    ///     ͷ  ť    ¼ 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnStartVideo_Click(object sender, EventArgs e)
    {
        try
        {
            // ڵ    ʼ  ʱ       ³ ʼ    ⣬  ֹ       ʱ      ͷ   ڵ      ͷ  ť֮ǰ      ͷ ε     
            initVideo();
            //   뱣֤ п       ͷ
            if (WebCamTexture.devices.Length == 0)
            {
                //MessageBox.Show("δ  ⵽    ͷ    ȷ   Ѱ װ    ͷ      !");
                Debug.LogError("δ  ⵽    ͷ    ȷ   Ѱ װ    ͷ      !");
                return;
            }
            //if (rgbVideoSource.IsRunning || irVideoSource.IsRunning)
            if (webCamTexture.isPlaying == true)
            {
                btnStartVideo.GetComponentInChildren<TMP_Text>().text = "        ͷ";

                //    // ر     ͷ
                //    if (irVideoSource.IsRunning)
                //    {
                //        irVideoSource.SignalToStop();
                //        irVideoSource.Hide();
                //    }
                //    if (rgbVideoSource.IsRunning)
                //    {
                //        rgbVideoSource.SignalToStop();
                //        rgbVideoSource.Hide();
                //    }
                webCamTexture.Stop();
                //  ѡ  ʶ  ͼ        ʼƥ 䡱  ť   ã   ֵ ؼ     
                //ControlsEnable(true, chooseImgBtn, matchBtn, chooseMultiImgBtn, btnClearFaceList);
                exitVideoRGBFR = true;
                exitVideoRGBLiveness = true;
                exitVideoIRFRLiveness = true;
            }
            else
            {
                if (isCompare)
                {
                    // ȶԽ     
                    for (int i = 0; i < leftImageFeatureList.Count; i++)
                    {
                        //imageList.Items[i].Text = string.Format("{0}  ", i);
                    }
                    //lblCompareInfo.Text = string.Empty;
                    isCompare = false;
                }
                //  ѡ  ʶ  ͼ        ʼƥ 䡱  ť   ã   ʾ    ͷ ؼ 
                //rgbVideoSource.Show();
                //irVideoSource.Show();
                //ControlsEnable(false, chooseImgBtn, matchBtn, chooseMultiImgBtn, btnClearFaceList);
                btnStartVideo.GetComponentInChildren<TMP_Text>().text = " ر     ͷ";
                //  ȡfilterInfoCollection      
                int maxCameraCount = WebCamTexture.devices.Length;
                //               ͬ      ͷ    
                if (rgbCameraIndex != irCameraIndex && maxCameraCount >= 2)
                {
                    //RGB    ͷ    
                    rgbDeviceVideo = new VideoCaptureDevice(filterInfoCollection[rgbCameraIndex < maxCameraCount ? rgbCameraIndex : 0].MonikerString);
                    //rgbVideoSource.VideoSource = rgbDeviceVideo;
                    //rgbVideoSource.Start();
                    webCamTexture.Play();

                    //IR    ͷ
                    irDeviceVideo = new VideoCaptureDevice(filterInfoCollection[irCameraIndex < maxCameraCount ? irCameraIndex : 0].MonikerString);
                    //irVideoSource.VideoSource = irDeviceVideo;
                    //irVideoSource.Start();
                    //webCamTexture2.Play();

                    //˫   ־  Ϊtrue
                    isDoubleShot = true;
                    //       ߳ 
                    exitVideoIRFRLiveness = false;
                    videoIRLiveness();
                }
                else
                {
                    //    RGB    ͷ  IR    ͷ ؼ     
                    rgbDeviceVideo = new VideoCaptureDevice(filterInfoCollection[rgbCameraIndex <= maxCameraCount ? rgbCameraIndex : 0].MonikerString);
                    //rgbVideoSource.VideoSource = rgbDeviceVideo;
                    //rgbVideoSource.Start();
                    //irVideoSource.Hide();
                    webCamTexture.Play();
                }
                //           ߳ 
                exitVideoRGBFR = false;
                exitVideoRGBLiveness = false;
                videoRGBLiveness();
                videoRGBFR();
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    /// RGB    ͷPaint ¼   ͼ    ʾ       ϣ  õ ÿһ֡ͼ 񣬲    д   
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void videoSource_Paint(object sender)
    {
        try
        {
            //if (!rgbVideoSource.IsRunning)
            {
                return;
            }
            // õ   ǰRGB    ͷ µ ͼƬ
            lock (rgbVideoImageLocker)
            {
                //rgbVideoBitmap = rgbVideoSource.GetCurrentVideoFrame();
            }
            Bitmap bitmapClone = null;
            try
            {
                lock (rgbVideoImageLocker)
                {
                    if (rgbVideoBitmap == null)
                    {
                        return;
                    }
                    bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                }
                if (bitmapClone == null)
                {
                    return;
                }
                //          õ Rect  
                MultiFaceInfo multiFaceInfo = FaceUtil.DetectFaceAndLandMark(videoEngine, bitmapClone);
                //δ  ⵽    
                if (multiFaceInfo.faceNum <= 0)
                {
                    trackRGBUnitDict.ClearAllElement();
                    return;
                }
                //Graphics g = e.Graphics;
                //float offsetX = rgbVideoSource.Width * 1f / bitmapClone.Width;
                //float offsetY = rgbVideoSource.Height * 1f / bitmapClone.Height;
                List<int> tempIdList = new List<int>();
                for (int faceIndex = 0; faceIndex < multiFaceInfo.faceNum; faceIndex++)
                {
                    MRECT mrect = multiFaceInfo.faceRects[faceIndex];
                    //float x = mrect.left * offsetX;
                    //float width = mrect.right * offsetX - x;
                    //float y = mrect.top * offsetY;
                    //float height = mrect.bottom * offsetY - y;
                    int faceId = multiFaceInfo.faceID[faceIndex];
                    FaceTrackUnit currentFaceTrack = trackRGBUnitDict.GetElementByKey(faceId);
                    //    Rect   л   
                    //    һ֡       ʾ  ҳ    
                    lock (rgbVideoImageLocker)
                    {
                        if (multiFaceInfo.pointAyy != null && multiFaceInfo.pointAyy.Length > 0)
                        {
                            ASF_FaceLandmark[] markAyy = multiFaceInfo.pointAyy[faceIndex];
                            if (markAyy != null && markAyy.Length > 0)
                            {
                                PointF[] points = new PointF[markAyy.Length];
                                if (markAyy.Length > 0)
                                {
                                    for (int markIndex = 0; markIndex < markAyy.Length; markIndex++)
                                    {
                                        //points[markIndex].X = markAyy[markIndex].x * offsetX;
                                        //points[markIndex].Y = markAyy[markIndex].y * offsetY;
                                    }
                                }
                                //g.DrawPolygon(Pens.Blue, points);
                            }
                        }
                        if (currentFaceTrack != null)
                        {
                            //g.DrawRectangle(currentFaceTrack.CertifySuccess() ? Pens.Green : Pens.Red, x, y, width, height);
                            //if (!string.IsNullOrWhiteSpace(currentFaceTrack.GetCombineMessage()) && x > 0 && y > 0)
                            {
                                //g.DrawString(currentFaceTrack.GetCombineMessage(), font, currentFaceTrack.CertifySuccess() ? greenBrush : redBrush, x, y - 15);
                            }
                        }
                        else
                        {
                            //g.DrawRectangle(Pens.Red, x, y, width, height);
                        }
                    }
                    if (faceId >= 0)
                    {
                        // ж faceid Ƿ            
                        if (!rgbFeatureTryDict.ContainsKey(faceId))
                        {
                            rgbFeatureTryDict.AddDictionaryElement(faceId, 0);
                        }
                        if (!rgbLivenessTryDict.ContainsKey(faceId))
                        {
                            rgbLivenessTryDict.AddDictionaryElement(faceId, 0);
                        }
                        if (trackRGBUnitDict.ContainsKey(faceId))
                        {
                            trackRGBUnitDict.GetElementByKey(faceId).Rect = mrect;
                            trackRGBUnitDict.GetElementByKey(faceId).FaceOrient = multiFaceInfo.faceOrients[faceIndex];
                            trackRGBUnitDict.GetElementByKey(faceId).FaceDataInfo = multiFaceInfo.faceDataInfoList[faceIndex];
                        }
                        else
                        {
                            trackRGBUnitDict.AddDictionaryElement(faceId, new FaceTrackUnit(faceId, mrect, multiFaceInfo.faceOrients[faceIndex], multiFaceInfo.faceDataInfoList[faceIndex]));
                        }
                        tempIdList.Add(faceId);
                    }

                }
                //  ʼ    ˢ ´        , Ƴ          
                rgbFeatureTryDict.RefershElements(tempIdList);
                rgbLivenessTryDict.RefershElements(tempIdList);
                trackRGBUnitDict.RefershElements(tempIdList);
            }
            catch (Exception ee)
            {
                LogUtil.LogInfo(GetType(), ee);
            }
            finally
            {
                if (bitmapClone != null)
                {
                    bitmapClone.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    ///        ߳ 
    /// </summary>
    private void videoRGBLiveness()
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
        {
            while (true)
            {
                if (exitVideoRGBLiveness)
                {
                    return;
                }
                if (rgbLivenessTryDict.GetDictCount() <= 0)
                {
                    continue;
                }
                try
                {
                    if (rgbVideoBitmap == null)
                    {
                        continue;
                    }
                    List<int> faceIdList = new List<int>();
                    faceIdList.AddRange(rgbLivenessTryDict.GetAllElement().Keys);
                    //        Id     л     
                    foreach (int tempFaceId in faceIdList)
                    {
                        //          в    ڣ  Ƴ 
                        if (!rgbLivenessTryDict.ContainsKey(tempFaceId))
                        {
                            continue;
                        }
                        //   ڳ  Դ      Ƴ 
                        int tryTime = rgbLivenessTryDict.GetElementByKey(tempFaceId);
                        if (tryTime >= liveMatchTime)
                        {
                            continue;
                        }
                        tryTime += 1;
                        // ޶ Ӧ          Ϣ
                        if (!trackRGBUnitDict.ContainsKey(tempFaceId))
                        {
                            continue;
                        }
                        FaceTrackUnit tempFaceTrack = trackRGBUnitDict.GetElementByKey(tempFaceId);

                        //RGB      
                        Console.WriteLine(string.Format("faceId:{0},       {1}  \r\n", tempFaceId, tryTime));
                        SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                        singleFaceInfo.faceOrient = tempFaceTrack.FaceOrient;
                        singleFaceInfo.faceRect = tempFaceTrack.Rect;
                        singleFaceInfo.faceDataInfo = tempFaceTrack.FaceDataInfo;
                        Bitmap bitmapClone = null;
                        try
                        {
                            lock (rgbVideoImageLocker)
                            {
                                if (rgbVideoBitmap == null)
                                {
                                    break;
                                }
                                bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                            }
                            int retCodeLiveness = -1;
                            LivenessInfo liveInfo = FaceUtil.LivenessInfo_RGB(videoRGBImageEngine, bitmapClone, singleFaceInfo, out retCodeLiveness);
                            //   »       
                            if (retCodeLiveness.Equals(0) && liveInfo.num > 0 && trackRGBUnitDict.ContainsKey(tempFaceId))
                            {
                                trackRGBUnitDict.GetElementByKey(tempFaceId).RgbLiveness = liveInfo.isLive[0];
                                if (liveInfo.isLive[0].Equals(1))
                                {
                                    tryTime = liveMatchTime;
                                }
                            }
                        }
                        catch (Exception ee)
                        {
                            LogUtil.LogInfo(GetType(), ee);
                        }
                        finally
                        {
                            if (bitmapClone != null)
                            {
                                bitmapClone.Dispose();
                            }
                        }
                        rgbLivenessTryDict.UpdateDictionaryElement(tempFaceId, tryTime);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo(GetType(), ex);
                }
            }
        }));
    }

    /// <summary>
    ///       ȡ       ߳ 
    /// </summary>
    private void videoRGBFR()
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
        {
            while (true)
            {
                if (exitVideoRGBFR)
                {
                    return;
                }
                if (rgbFeatureTryDict.GetDictCount() <= 0)
                {
                    continue;
                }
                //         Ϊ  ʱ     ý           
                if (leftImageFeatureList.Count <= 0)
                {
                    continue;
                }
                try
                {
                    if (rgbVideoBitmap == null)
                    {
                        continue;
                    }
                    List<int> faceIdList = new List<int>();
                    faceIdList.AddRange(rgbFeatureTryDict.GetAllElement().Keys);
                    foreach (int tempFaceId in faceIdList)
                    {
                        //          в    ڣ  Ƴ 
                        if (!rgbFeatureTryDict.ContainsKey(tempFaceId))
                        {
                            continue;
                        }
                        //   ڳ  Դ      Ƴ 
                        int tryTime = rgbFeatureTryDict.GetElementByKey(tempFaceId);
                        if (tryTime >= frMatchTime)
                        {
                            continue;
                        }
                        // ޶ Ӧ          Ϣ
                        if (!trackRGBUnitDict.ContainsKey(tempFaceId))
                        {
                            continue;
                        }
                        FaceTrackUnit tempFaceTrack = trackRGBUnitDict.GetElementByKey(tempFaceId);
                        tryTime += 1;
                        //        
                        int faceIndex = -1;
                        float similarity = 0f;
                        Console.WriteLine(string.Format("faceId:{0},          {1}  \r\n", tempFaceId, tryTime));
                        //  ȡ        
                        SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                        singleFaceInfo.faceOrient = tempFaceTrack.FaceOrient;
                        singleFaceInfo.faceRect = tempFaceTrack.Rect;
                        singleFaceInfo.faceDataInfo = tempFaceTrack.FaceDataInfo;
                        Bitmap bitmapClone = null;
                        try
                        {
                            lock (rgbVideoImageLocker)
                            {
                                if (rgbVideoBitmap == null)
                                {
                                    break;
                                }
                                bitmapClone = (Bitmap)rgbVideoBitmap.Clone();

                                //        Bitmap ͬ ߴ   Texture2D  Ĭ  ʹ   RGBA32   ʽ  ֧  ͸    
                                Texture2D texture = new Texture2D(bitmapClone.Width, bitmapClone.Height, TextureFormat.RGBA32, false);

                                //      Bitmap          ݣ   ȡԭʼ ֽ     
                                Rectangle rect = new Rectangle(0, 0, bitmapClone.Width, bitmapClone.Height);
                                BitmapData bmpData = bitmapClone.LockBits(rect, ImageLockMode.ReadOnly, bitmapClone.PixelFormat);

                                //            ݴ С  ÿ   ֽ        ߶ȣ 
                                int byteCount = bmpData.Stride * bitmapClone.Height;
                                byte[] bitmapBytes = new byte[byteCount];

                                //    Bitmap        ݸ  Ƶ  ֽ     
                                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bitmapBytes, 0, byteCount);

                                //      Bitmap
                                bitmapClone.UnlockBits(bmpData);

                                // ע ⣺Bitmap      ظ ʽ       BGR    BGRA     Unity  ڴ      RGBA    Ҫת  ͨ  
                                //    磺   BGR ת  Ϊ RGB     BGRA ת  Ϊ RGBA
                                for (int i = 0; i < bitmapBytes.Length; i += 4)
                                {
                                    byte temp = bitmapBytes[i];          //      B ͨ  ֵ
                                    bitmapBytes[i] = bitmapBytes[i + 2]; // R ͨ   滻 B ͨ  
                                    bitmapBytes[i + 2] = temp;          // B ͨ   滻 R ͨ  
                                                                        //      A ͨ    i+3        ת  
                                }

                                //          ֽ       ص  Texture2D
                                texture.LoadRawTextureData(bitmapBytes);
                                texture.Apply();

                                rawImage.texture = texture;
                            }
                            FaceFeature feature = FaceUtil.ExtractFeature(videoRGBImageEngine, bitmapClone, singleFaceInfo);
                            if (feature == null || feature.featureSize <= 0)
                            {
                                break;
                            }
                            //        
                            faceIndex = compareFeature(feature, out similarity);
                            //   ±ȶԽ  
                            if (trackRGBUnitDict.ContainsKey(tempFaceId))
                            {
                                trackRGBUnitDict.GetElementByKey(tempFaceId).SetFaceIndexAndSimilarity(faceIndex, similarity.ToString("#0.00"));
                                if (faceIndex > -1)
                                {
                                    tryTime = frMatchTime;
                                }
                            }
                        }
                        catch (Exception ee)
                        {
                            LogUtil.LogInfo(GetType(), ee);
                        }
                        finally
                        {
                            if (bitmapClone != null)
                            {
                                bitmapClone.Dispose();
                            }
                        }
                        rgbFeatureTryDict.UpdateDictionaryElement(tempFaceId, tryTime);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo(GetType(), ex);
                }
            }
        }));
    }

    /// <summary>
    /// IR    ͷPaint ¼ ,ͬ  RGB     򣬶Ա           IR      
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void irVideoSource_Paint(object sender)
    {
        try
        {
            //if (!isDoubleShot || !irVideoSource.IsRunning)
            {
                return;
            }
            //   ˫ 㣬  IR    ͷ        ȡIR    ͷͼƬ
            lock (irVideoImageLocker)
            {
                //irVideoBitmap = irVideoSource.GetCurrentVideoFrame();
            }
            Bitmap irBmpClone = null;
            try
            {
                lock (irVideoImageLocker)
                {
                    if (irVideoBitmap == null)
                    {
                        return;
                    }
                    irBmpClone = (Bitmap)irVideoBitmap.Clone();
                }
                if (irBmpClone == null)
                {
                    return;
                }
                //У  ͼƬ   
                CheckBitmapWidthAndHeight(ref irBmpClone);
                //          õ Rect  
                MultiFaceInfo multiFaceInfo = FaceUtil.DetectFaceIR(videoIRImageEngine, irBmpClone);
                if (multiFaceInfo.faceNum <= 0)
                {
                    trackIRUnit.FaceId = -1;
                    return;
                }
                // õ        
                SingleFaceInfo irMaxFace = FaceUtil.GetMaxFace(multiFaceInfo);
                // õ Rect
                MRECT rect = irMaxFace.faceRect;
                //   RGB    ͷ         
                //Graphics g = e.Graphics;
                //float offsetX = irVideoSource.Width * 1f / irBmpClone.Width;
                //float offsetY = irVideoSource.Height * 1f / irBmpClone.Height;
                //float x = rect.left * offsetX;
                //float width = rect.right * offsetX - x;
                //float y = rect.top * offsetY;
                //float height = rect.bottom * offsetY - y;
                //    Rect   л   
                lock (irVideoImageLocker)
                {
                    //g.DrawRectangle(trackIRUnit.IrLiveness.Equals(1) ? Pens.Green : Pens.Red, x, y, width, height);
                    //if (!string.IsNullOrWhiteSpace(trackIRUnit.GetIrLivenessMessage()) && x > 0 && y > 0)
                    {
                        //    һ֡       ʾ  ҳ    
                        //g.DrawString(trackIRUnit.GetIrLivenessMessage(), font, trackIRUnit.IrLiveness.Equals(1) ? greenBrush : redBrush, x, y - 15);
                    }
                }
                trackIRUnit.Rect = irMaxFace.faceRect;
                trackIRUnit.FaceOrient = irMaxFace.faceOrient;
                trackIRUnit.FaceDataInfo = irMaxFace.faceDataInfo;
            }
            catch (Exception ee)
            {
                LogUtil.LogInfo(GetType(), ee);
            }
            finally
            {
                if (irBmpClone != null)
                {
                    irBmpClone.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    /// IR       ߳ 
    /// </summary>
    private void videoIRLiveness()
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
        {
            while (true)
            {
                if (exitVideoIRFRLiveness)
                {
                    return;
                }
                try
                {
                    if (trackIRUnit.FaceDataInfo == null)
                    {
                        continue;
                    }
                    Bitmap bitmapClone = null;
                    try
                    {
                        lock (irVideoImageLocker)
                        {
                            if (irVideoBitmap == null)
                            {
                                continue;
                            }
                            bitmapClone = (Bitmap)irVideoBitmap.Clone();
                            if (bitmapClone == null)
                            {
                                continue;
                            }
                        }
                        SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                        singleFaceInfo.faceOrient = trackIRUnit.FaceOrient;
                        singleFaceInfo.faceRect = trackIRUnit.Rect;
                        singleFaceInfo.faceDataInfo = trackIRUnit.FaceDataInfo;
                        int retCodeLiveness = -1;
                        LivenessInfo liveInfo = FaceUtil.LivenessInfo_IR(videoIRImageEngine, bitmapClone, singleFaceInfo, out retCodeLiveness);
                        if (retCodeLiveness.Equals(0) && liveInfo.num > 0)
                        {
                            trackIRUnit.IrLiveness = liveInfo.isLive[0];
                        }
                    }
                    catch (Exception ee)
                    {
                        LogUtil.LogInfo(GetType(), ee);
                    }
                    finally
                    {
                        if (bitmapClone != null)
                        {
                            bitmapClone.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo(GetType(), ex);
                }
            }
        }));
    }

    /// <summary>
    ///  õ feature ȽϽ  
    /// </summary>
    /// <param name="feature"></param>
    /// <returns></returns>
    private int compareFeature(FaceFeature feature, out float similarity)
    {
        int result = -1;
        similarity = 0f;
        try
        {
            //        ⲻΪ գ          ƥ  
            if (leftImageFeatureList != null && leftImageFeatureList.Count > 0)
            {
                for (int i = 0; i < leftImageFeatureList.Count; i++)
                {
                    //        ƥ ䷽        ƥ  
                    videoRGBImageEngine.ASFFaceFeatureCompare(feature, leftImageFeatureList[i], out similarity, compareModel);
                    if (similarity >= threshold)
                    {
                        result = i;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
        return result;
    }

    /// <summary>
    ///     ͷ        ¼ 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="reason"></param>
    private void videoSource_PlayingFinished(object sender, AForge.Video.ReasonToFinishPlaying reason)
    {
        try
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
            //ControlsEnable(true, chooseImgBtn, matchBtn, chooseMultiImgBtn, btnClearFaceList);
            exitVideoRGBFR = true;
            exitVideoRGBLiveness = true;
            exitVideoIRFRLiveness = true;
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    #endregion

    #region       ֵ   
    /// <summary>
    ///   ֵ ı         ¼               Ƿ   ȷ      ȷ        
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void txtThreshold_KeyPress(object sender)
    {
        try
        {
            //  ֹ Ӽ        
            //e.Handled = true;
            //    ֵ       ˼   .     룬            
            //if (char.IsDigit(e.KeyChar) || e.KeyChar == 8 || e.KeyChar == '.')
            {
                //      ǰ ı        
                string thresholdStr = "0";// txtThreshold.Text.Trim();
                int countStr = 0;
                int startIndex = 0;
                //     ǰ          Ƿ  ǡ .  
                //if (e.KeyChar == '.')
                {
                    countStr = 1;
                }
                //  ⵱ǰ     Ƿ   . ĸ   
                if (thresholdStr.IndexOf('.', startIndex) > -1)
                {
                    countStr += 1;
                }
                //             Ѿ     12   ַ   
                //if (e.KeyChar != 8 && (thresholdStr.Length > 12 || countStr > 1))
                {
                    return;
                }
                //e.Handled = false;
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    ///      ȶ   ֵ ı    ̧   ¼        ֵ Ƿ   ȷ      ȷ  Ϊ0.8f
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void txtThreshold_KeyUp(object sender)
    {
        //textBoxKeyUp(txtThreshold, ref threshold, 0.80f);
        AppendText(string.Format("      Ƶģʽ ȶ   ֵ ɹ ,  ֵ:{0}", threshold));
    }

    /// <summary>
    ///     (IR)      ֵ ı   ̧   ¼       ȷ  ΪĬ  ֵ
    /// </summary>
    private void txtIr_KeyUp(object sender)
    {
        try
        {
            //textBoxKeyUp(txtIr, ref thresholdIr, 0.7f);
            //   »     ֵ
            int retCode = videoRGBImageEngine.ASFSetLivenessParam(thresholdIr);
            AppendText(string.Format("   º   (IR)      ֵ{0},  ֵ:{1}", retCode.Equals(0) ? " ɹ " : "ʧ  , ӿڷ   ֵ" + retCode, thresholdIr));
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }

    }

    /// <summary>
    ///  ɼ   (RGB)      ֵ ı   ̧   ¼       ȷ  ΪĬ  ֵ
    /// </summary>
    private void txtRgb_KeyUp(object sender)
    {
        try
        {
            //textBoxKeyUp(txtRgb, ref thresholdRgb, 0.5f);
            //   »     ֵ
            int retCode = videoRGBImageEngine.ASFSetLivenessParam(thresholdRgb);
            AppendText(string.Format("   ¿ɼ   (RGB)      ֵ{0},  ֵ:{1}", retCode.Equals(0) ? " ɹ " : "ʧ  , ӿڷ   ֵ" + retCode, thresholdRgb));
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    ///ͼ      ע    ֵ ı   ̧   ¼       ȷ  ΪĬ  ֵ
    /// </summary>
    private void txtImgQua_KeyUp(object sender)
    {
        //textBoxKeyUp(txtImgQua, ref thresholdImgRegister, 0.63f);
        AppendText(string.Format("    ͼ      ע    ֵ ɹ ,  ֵ:{0}", thresholdImgRegister));
    }

    /// <summary>
    /// ͼ      ʶ         ֵ ı   ̧   ¼       ȷ  ΪĬ  ֵ
    /// </summary>
    private void txtImgMask_KeyUp(object sender)
    {
        //textBoxKeyUp(txtImgMask, ref thresholdImgMask, 0.49f);
        AppendText(string.Format("    ͼ      ʶ         ֵ ɹ ,  ֵ:{0}", thresholdImgMask));
    }

    /// <summary>
    /// ͼ      ʶ  δ        ֵ ı   ̧   ¼       ȷ  ΪĬ  ֵ
    /// </summary>
    private void txtImgNoMask_KeyUp(object sender)
    {
        //textBoxKeyUp(txtImgNoMask, ref thresholdImgNoMask, 0.29f);
        AppendText(string.Format("    ͼ      ʶ  δ        ֵ ɹ ,  ֵ:{0}", thresholdImgNoMask));
    }

    /// <summary>
    /// ̧   ¼     
    /// </summary>
    /// <param name="textBox"> ı      </param>
    /// <param name="value">  ֵ    </param>
    /// <param name="defaultValue">Ĭ  ֵ</param>
    private void textBoxKeyUp(ref float value, float defaultValue)
    {
        try
        {
            //           ݲ   ȷ  ΪĬ  ֵ
            //if (!float.TryParse(textBox.Text.Trim(), out value))
            {
                value = defaultValue;
            }
            if (value > 1)
            {
                value = 1;
            }
            if (value < 0)
            {
                value = 0;
            }
            //textBox.Text = value.ToString();
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region     ر 
    /// <summary>
    ///     ر  ¼ 
    /// </summary>
    private void Form_Closed(object sender)
    {
        try
        {
            //if (rgbVideoSource.IsRunning)
            {
                btnStartVideo_Click(sender, new EventArgs()); // ر     ͷ
            }
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region    ÷   
    /// <summary>
    ///  ָ ʹ  /   ÿؼ  б ؼ 
    /// </summary>
    /// <param name="isEnable"></param>
    /// <param name="controls"> ؼ  б </param>
    private void ControlsEnable(bool isEnable)
    {
        try
        {
            //if (controls == null || controls.Length <= 0)
            {
                return;
            }
            //foreach (Control control in controls)
            {
                //control.Enabled = isEnable;
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    /// У  ͼƬ
    /// </summary>
    /// <param name="imagePath"></param>
    /// <returns></returns>
    private bool CheckImage(string imagePath)
    {
        try
        {
            if (imagePath == null)
            {
                AppendText("ͼƬ     ڣ   ȷ Ϻ  ٵ   ");
                return false;
            }
            try
            {
                // ж ͼƬ Ƿ        罫     ļ  Ѻ ׺  Ϊ.jpg       ͻᱨ  
                Image image = ImageUtil.ReadFromFile(imagePath);
                if (image == null)
                {
                    throw new ArgumentException(" image is null");
                }
                else
                {
                    image.Dispose();
                }
            }
            catch
            {
                AppendText(string.Format("{0} ͼƬ  ʽ     ⣬  ȷ Ϻ  ٵ   ", imagePath));
                return false;
            }
            FileInfo fileCheck = new FileInfo(imagePath);
            if (!fileCheck.Exists)
            {
                AppendText(string.Format("{0}       ", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length > maxSize)
            {
                AppendText(string.Format("{0} ͼƬ  С    2M    ѹ     ٵ   ", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length < 2)
            {
                AppendText(string.Format("{0} ͼ      ̫С        ѡ  ", fileCheck.Name));
                return false;
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
        return true;
    }

    /// <summary>
    /// ׷ ӹ  ÷   
    /// </summary>
    /// <param name="message"></param>
    private void AppendText(string message)
    {
        try
        {
            webCamDisplayText.text += message + "\n";
            //logBox.AppendText(message + "\r\n");
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    ///    ͼƬ   
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    private void CheckImageWidthAndHeight(ref Image image)
    {
        if (image == null)
        {
            return;
        }
        try
        {
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                image = ImageUtil.ScaleImage(image, maxWidth, maxHeight);
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }

    /// <summary>
    ///    ͼƬ   
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    private void CheckBitmapWidthAndHeight(ref Bitmap bitmap)
    {
        if (bitmap == null)
        {
            return;
        }
        try
        {
            if (bitmap.Width > maxWidth || bitmap.Height > maxHeight)
            {
                bitmap = (Bitmap)ImageUtil.ScaleImage(bitmap, maxWidth, maxHeight);
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion

    #region  л  ȶ ģʽ            Ϣ ļ  Ͱ汾  Ϣ
    /// <summary>
    ///     -   ʺ      -        
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FaceForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
    {
        //     Ӿ     ƽ̨-        
        System.Diagnostics.Process.Start("https://ai.arcsoft.com.cn/ucenter/resource/build/index.html#/help");
    }

    /// <summary>
    ///  л  ȶ ģʽ
    /// </summary>
    private void btnCompareModel_Click(object sender, EventArgs e)
    {
        bool isIdMode = compareModel.Equals(ASF_CompareModel.ASF_ID_PHOTO);
        if (isIdMode)
        {
            compareModel = ASF_CompareModel.ASF_ID_PHOTO;
        }
        else
        {
            compareModel = ASF_CompareModel.ASF_LIFE_PHOTO;
        }
        AppendText(string.Format("  ǰ ȶ ģʽ:{0}", isIdMode ? "ASF_ID_PHOTO" : "ASF_LIFE_PHOTO"));
    }

    /// <summary>
    ///           Ϣ ļ  Ͱ汾  Ϣ
    /// </summary>
    private void picExport_Click(object sender, EventArgs e)
    {
        try
        {
            ActiveFileInfo activeFileInfo;
            int retCode = imageEngine.ASFGetActiveFileInfo(out activeFileInfo);
            SDKVersion sdkVersion;
            imageEngine.ASFGetVersion(out sdkVersion);
            if (retCode != 0)
            {
                //MessageBox.Show("  ȡ     ļ   Ϣʧ  ,      :" + retCode);
                return;
            }
            //SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = " ı  ļ   *.txt  |*.txt";
            //sfd.Title = "   漤   ļ   SDK 汾  Ϣ";
            //sfd.FilterIndex = 1;
            //sfd.RestoreDirectory = true;
            //sfd.FileName = "ActiveFileAndSdkVersionInfo.txt";
            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            //    string localFilePath = sfd.FileName.ToString();
            //    File.WriteAllText(localFilePath, activeFileInfo.ToString());
            //    File.AppendAllText(localFilePath, sdkVersion.ToString());
            //}
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo(GetType(), ex);
        }
    }
    #endregion
}
