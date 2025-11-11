using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _014.Utilities.UI
{
    internal class Notes
    {
        /*
         

        Artık her yeni class/method oluşturduğunuzda:

Visual Studio'da /// yazın ve ENTER'a basın
Şablon otomatik oluşur
Doldur (summary, param, returns, remarks)
Bitti! ✅

Şablon dosyası: Her zaman XML_DOKUMANTASYON_SABLONU.md dosyasına bakabilirsiniz.


      
         Skip to main content
Logo
Submit a request
2 yalcin cinar
Help Center 3D Graphics General
Search...
Rendered Mesh in Wireframe display mode

Antonio Spagnuolo
Updated 2 years ago
Not yet followed by anyone
The proposed solution is not natively supported and may not work in all scenarios or versions.

You can have a Mesh entity displayed always as Rendered, regardless of the active display mode, by using the following class derived from Mesh:



public class MyShadedMesh : Mesh
{
    public MyShadedMesh(Point3D[] vertices, IndexTriangle[] triangles) : base(vertices, triangles)
    {
    }

    public override void DrawWireframe(DrawParams data)
    {
        Pre(data);
        SetRenderedShader(data);

        Color col = Color;

        if (data.Selected)
        {
            col = Color.Gold; // sets the selection color
        }

        data.RenderContext.SetColorDiffuse(col, col);

        Draw(data);

        Post(data);
    }

    void SetRenderedShader(DrawParams data)
    {
        if (data.ShaderParams != null)
        {
            bool prevLighting = data.ShaderParams.Lighting;
            var prevPrimitiveType = data.ShaderParams.PrimitiveType;

            data.ShaderParams.Lighting = true;
            data.ShaderParams.PrimitiveType = shaderPrimitiveType.Polygon;

            base.SetShader(data);

            data.ShaderParams.Lighting = prevLighting;
            data.ShaderParams.PrimitiveType = prevPrimitiveType;
        }
    }

    public override void DrawIsocurves(DrawParams data)
    {
        // intentionally left blank
    }

    public override void DrawForSelection(DrawForSelectionParams data)
    {
        Pre(data);

        base.DrawForSelection(data);

        Post(data);
    }

    private void Pre(DrawParams data)
    {
        data.RenderContext.PushRasterizerState();
        data.RenderContext.SetState(rasterizerStateType.CCW_PolygonFill_CullFaceBack_PolygonOffset_1_1);
    }

    private static void Post(DrawParams data)
    {
        data.RenderContext.PopRasterizerState();
    }
}
Usage:

design1.ActiveViewport.DisplayMode = displayType.Wireframe;

Mesh sphere = Mesh.CreateSphere(10, 24, 12);
design1.Entities.Add(new MyShadedMesh(sphere.Vertices, sphere.Triangles), Color.Red);

Mesh cone = Mesh.CreateCone(10, 0, new Point3D(20, 0, 0), new Point3D(20, 0, 20), 16);
design1.Entities.Add(cone, Color.Green);
Was this article helpful?
 
0 out of 0 found this helpful
Comments
0 comments
Be the first to write a comment.


Articles in this section
Infinite Grid
EDM Simulation using MultiFastMesh
MultiFastMesh
Custom Entity with EntityGraphicsData
Limitations of AntiAliasing
Fast and approximate world position under mouse cursor
Screen-Space Ambient Occlusion
Image-based Silhouettes
Limitations of TempEntities
Performance Tips
© 2025 devDept Software S.r.l.

Twitter YouTube LinkedIn Github
Return to top










        */
    }
}
