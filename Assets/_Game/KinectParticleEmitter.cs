using UnityEngine;

public class DepthParticlesFromViewer : MonoBehaviour
{
    public DepthImageViewer depthViewer; // Arraste o componente DepthImageViewer aqui
    public ParticleSystem particleSystemRef;

    [Range(1, 16)]
    public int pixelStep = 6; // Quanto maior, menos partículas e melhor a performance
    public float depthScale = 0.002f; // Multiplicador para profundidade Z

    private ParticleSystem.Particle[] particles;

    void Start()
    {
        if (depthViewer == null)
            depthViewer = GetComponent<DepthImageViewer>();

        // Aloca array de partículas (máximo baseado na resolução típica do Kinect 320x240)
        particles = new ParticleSystem.Particle[320 * 240];
    }

    void Update()
    {
        if (depthViewer == null) return;

        // Pega a textura gerada pelo DepthImageViewer
        Texture2D depthTex = depthViewer.GetComponent<Renderer>()?.material.mainTexture as Texture2D;
        if (depthTex == null) return;

        Color32[] pixels = depthTex.GetPixels32();
        int width = depthTex.width;
        int height = depthTex.height;

        int particleCount = 0;

        for (int y = 0; y < height; y += pixelStep)
        {
            for (int x = 0; x < width; x += pixelStep)
            {
                int index = x + y * width;
                Color32 pixelColor = pixels[index];

                // Considera um pixel como "Usuário/Corpo" se ele não for totalmente transparente ou preto
                if (pixelColor.a > 10 && (pixelColor.r > 10 || pixelColor.g > 10 || pixelColor.b > 10))
                {
                    if (particleCount < particles.Length)
                    {
                        // Mapeia os pixels X e Y para coordenadas locais/mundo
                        float posX = (x - (width / 2f)) * 0.01f;
                        float posY = (y - (height / 2f)) * 0.01f;

                        // Opcional: usa o brilho do pixel como profundidade Z
                        float posZ = pixelColor.r * depthScale;

                        particles[particleCount].position = new Vector3(posX, posY, posZ);
                        particles[particleCount].startColor = pixelColor; // Mantém a cor do DepthImageViewer
                        particles[particleCount].startSize = 0.04f;

                        particleCount++;
                    }
                }
            }
        }

        // Aplica o array atualizado de partículas ao Particle System
        particleSystemRef.SetParticles(particles, particleCount);
    }
}
