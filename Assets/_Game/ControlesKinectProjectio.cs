using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ControlesKinectProjection : MonoBehaviour
{
    [Header("Referências")]
    public Material materialKinect;
    public VideoPlayer videoPlayer;

    [Header("Calibração Espacial da Máscara")]
    public float velocidadeMovimento = 0.5f;
    public float velocidadeEscala = 0.5f;

    private List<string> listaVideos = new List<string>();
    private int videoAtualIndex = 0;

    // Variáveis que vão alimentar o Shader
    private float zoomFactor = 1.0f;
    private Vector2 panOffset = Vector2.zero;

    void Start()
    {
        CarregarVideos();
        if (listaVideos.Count > 0)
        {
            videoPlayer.source = VideoSource.Url;
            TocarVideoIndex(0);
        }
        AtualizarCalibracaoShader();
    }

    void Update()
    {
        // --- TROCA DE VÍDEOS (Setas) ---
        if (listaVideos.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) ProximoVideo();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) VideoAnterior();
        }

        // --- CALIBRAÇÃO ESPACIAL NO SHADER (W, A, S, D e + / -) ---
        bool recalibrar = false;

        if (Input.GetKey(KeyCode.W)) { panOffset.y -= velocidadeMovimento * Time.deltaTime; recalibrar = true; }
        if (Input.GetKey(KeyCode.S)) { panOffset.y += velocidadeMovimento * Time.deltaTime; recalibrar = true; }
        if (Input.GetKey(KeyCode.A)) { panOffset.x += velocidadeMovimento * Time.deltaTime; recalibrar = true; }
        if (Input.GetKey(KeyCode.D)) { panOffset.x -= velocidadeMovimento * Time.deltaTime; recalibrar = true; }

        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
        {
            zoomFactor += velocidadeEscala * Time.deltaTime;
            recalibrar = true;
        }
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            zoomFactor -= velocidadeEscala * Time.deltaTime;
            zoomFactor = Mathf.Max(0.1f, zoomFactor);
            recalibrar = true;
        }

        if (recalibrar) AtualizarCalibracaoShader();
    }

    private void AtualizarCalibracaoShader()
    {
        if (materialKinect == null) return;

        Vector2 tiling = new Vector2(1f / zoomFactor, 1f / zoomFactor);
        Vector2 offsetCentralizado = new Vector2((1f - tiling.x) / 2f, (1f - tiling.y) / 2f);
        Vector2 offsetFinal = offsetCentralizado + panOffset;

        materialKinect.SetVector("_Zoom", tiling);
        materialKinect.SetVector("_Pan", offsetFinal);
    }

    private void CarregarVideos()
    {
        listaVideos.Clear();
        string caminho = Path.Combine(Path.GetDirectoryName(Application.dataPath), "videos");
        if (!Directory.Exists(caminho)) Directory.CreateDirectory(caminho);

        DirectoryInfo dir = new DirectoryInfo(caminho);
        if (dir.Exists)
        {
            string[] formatos = { ".mp4", ".mov", ".avi", ".mkv" };
            foreach (FileInfo arq in dir.GetFiles("*.*"))
            {
                if (formatos.Contains(arq.Extension.ToLower())) listaVideos.Add(arq.FullName);
            }
        }
    }

    private void ProximoVideo() { videoAtualIndex = (videoAtualIndex + 1) % listaVideos.Count; TocarVideoIndex(videoAtualIndex); }
    private void VideoAnterior() { videoAtualIndex = (videoAtualIndex - 1 + listaVideos.Count) % listaVideos.Count; TocarVideoIndex(videoAtualIndex); }
    private void TocarVideoIndex(int index) { videoPlayer.url = listaVideos[index]; videoPlayer.Play(); }
}
