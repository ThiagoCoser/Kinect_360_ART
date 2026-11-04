using UnityEngine;

public class ConexaoKinect : MonoBehaviour
{
    [Header("Material com o Shader do Kinect")]
    public Material materialShader;

    void Update()
    {
        // Garante que o gestor oficial do Kinect V1 está na cena e funcionando
        if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
        {
            // Pega a textura ao vivo (Fundo preto, pessoas coloridas)
            Texture2D mascaraKinect = KinectManager.Instance.GetUsersLblTex();

            // Injeta no Shader
            if (mascaraKinect != null && materialShader != null)
            {
                materialShader.SetTexture("_Webcam", mascaraKinect);
            }
        }
    }
}
