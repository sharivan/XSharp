using NLua;
using SharpDX.Direct3D9;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Windows;
using System.Configuration;
using System.Reflection;
using System.Windows.Forms;
using XSharp.Engine.Graphics;
using XSharp.Engine.Input;
using XSharp.Engine.Sound;
using XSharp.Engine.World;
using XSharp.Graphics;
using XSharp.Interop;
using static XSharp.Engine.Consts;
using Box = XSharp.Math.Fixed.Geometry.Box;
using Color = XSharp.Graphics.Color;
using Configuration = System.Configuration.Configuration;
using D3D9LockFlags = SharpDX.Direct3D9.LockFlags;
using DataStream = XSharp.Graphics.DataStream;
using Device9 = SharpDX.Direct3D9.Device;
using DeviceType = SharpDX.Direct3D9.DeviceType;
using DX9Format = SharpDX.Direct3D9.Format;
using DX9Matrix = SharpDX.Matrix;
using DXSprite = SharpDX.Direct3D9.Sprite;
using Font = SharpDX.Direct3D9.Font;
using FontDescription = XSharp.Graphics.FontDescription;
using FontDrawFlags = XSharp.Graphics.FontDrawFlags;
using Format = XSharp.Graphics.Format;
using Matrix = XSharp.Math.Geometry.Matrix;
using Point = XSharp.Math.Geometry.Point;
using PresentFlags = SharpDX.Direct3D9.PresentFlags;
using PresentParameters = SharpDX.Direct3D9.PresentParameters;
using RectangleF = XSharp.Math.Geometry.RectangleF;
using ResultCode = SharpDX.Direct3D9.ResultCode;
using Size2F = XSharp.Math.Geometry.Size2F;
using SwapEffect = SharpDX.Direct3D9.SwapEffect;
using Usage = SharpDX.Direct3D9.Usage;
using Vector2 = XSharp.Math.Geometry.Vector2;
using Vector4 = SharpDX.Vector4;

namespace XSharp.Engine;

public class DX9Engine : BaseEngine
{
    new public static DX9Engine Engine => (DX9Engine) BaseEngine.Engine;

    public const VertexFormat D3DFVF_TLVERTEX = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture1;
    public const int VERTEX_SIZE = 5 * sizeof(float) + sizeof(int);

    private PresentParameters presentationParams;
    private DXSprite sprite;

    private EffectHandle psFadingLevelHandle;
    private EffectHandle psFadingColorHandle;
    private EffectHandle plsFadingLevelHandle;
    private EffectHandle plsFadingColorHandle;

    private DirectInput directInput;

    private IRenderTarget[] renderTargets = new IRenderTarget[16];

    private Lua lua;

    public Control Control
    {
        get;
        private set;
    }

    public Direct3D Direct3D
    {
        get;
        private set;
    }

    public Device9 Device
    {
        get;
        private set;
    }

    public VertexBuffer VertexBuffer
    {
        get;
        private set;
    }

    public PixelShader PixelShader
    {
        get;
        private set;
    }

    public PixelShader PaletteShader
    {
        get;
        private set;
    }

    public override string Title
    {
        get => Control.Text;
        set => Control.Text = value;
    }

    public override Size2F ClientSize
    {
        get
        {
            var size = Control.Size;
            return new Size2F(size.Width, size.Height);
        }
    }

    protected override WaveStreamFactory CreateWaveStreamUtil()
    {
        return new NAudioWaveStreamFactory();
    }

    protected override ITexture CreateStageTexture()
    {
        return new DX9Texture(Device, (int) StageSize.X, (int) StageSize.Y, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
    }

    protected override void InitGraphicDevice()
    {
        // Creates the Device
        var device = new Device9(Direct3D, 0, DeviceType.Hardware, Control.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.Multithreaded, presentationParams);
        Device = device;

        var function = ShaderBytecode.CompileFromFile("PixelShader.hlsl", "main", "ps_2_0");
        PixelShader = new PixelShader(device, function);

        psFadingLevelHandle = PixelShader.Function.ConstantTable.GetConstantByName(null, "fadingLevel");
        psFadingColorHandle = PixelShader.Function.ConstantTable.GetConstantByName(null, "fadingColor");

        function = ShaderBytecode.CompileFromFile("PaletteShader.hlsl", "main", "ps_2_0");
        PaletteShader = new PixelShader(device, function);

        plsFadingLevelHandle = PaletteShader.Function.ConstantTable.GetConstantByName(null, "fadingLevel");
        plsFadingColorHandle = PaletteShader.Function.ConstantTable.GetConstantByName(null, "fadingColor");

        device.VertexShader = null;
        device.PixelShader = PixelShader;
        device.VertexFormat = D3DFVF_TLVERTEX;

        VertexBuffer = new VertexBuffer(device, VERTEX_SIZE * 6, Usage.WriteOnly, D3DFVF_TLVERTEX, Pool.Managed);

        device.SetRenderState(RenderState.ZEnable, false);
        device.SetRenderState(RenderState.Lighting, false);
        device.SetRenderState(RenderState.AlphaBlendEnable, true);
        device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
        device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        device.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
        device.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);
        device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        sprite = new DXSprite(device);

        SetupQuad(VertexBuffer, (float) StageSize.X, (float) StageSize.Y);
    }

    protected override IKeyboard CreateKeyboard()
    {
        var result = new Keyboard(directInput);
        result.Properties.BufferSize = 2048;
        result.Acquire();
        return new DX9Keyboard(result);
    }

    protected override IJoystick CreateJoystick()
    {
        var joystickGuid = Guid.Empty;
        foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            joystickGuid = deviceInstance.InstanceGuid;

        if (joystickGuid == Guid.Empty)
        {
            foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;
        }

        if (joystickGuid != Guid.Empty)
        {
            var result = new Joystick(directInput, joystickGuid);
            result.Properties.BufferSize = 2048;
            result.Acquire();
            return new DX9Joystick(result);
        }

        return null;
    }

    protected override void Initialize(dynamic initializers)
    {
        Control = initializers.control;

        lua = new Lua();

        presentationParams = new PresentParameters
        {
            Windowed = !FULL_SCREEN,
            SwapEffect = SwapEffect.Discard,
            PresentationInterval = VSYNC ? PresentInterval.One : PresentInterval.Immediate,
            FullScreenRefreshRateInHz = FULL_SCREEN ? TICKRATE : 0,
            AutoDepthStencilFormat = DX9Format.D16,
            EnableAutoDepthStencil = true,
            BackBufferCount = DOUBLE_BUFFERED ? 2 : 1,
            BackBufferFormat = DX9Format.X8R8G8B8,
            BackBufferHeight = Control.ClientSize.Height,
            BackBufferWidth = Control.ClientSize.Width,
            PresentFlags = VSYNC ? PresentFlags.LockableBackBuffer : PresentFlags.None
        };

        Direct3D = new Direct3D();

        directInput = new DirectInput();

        lua.LoadCLRPackage(); // TODO : This can be DANGEROUS! Fix in the future by adding restrictions on the scripting.
        lua.DoString(@"import ('XSharp', 'XSharp')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Effects')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Enemies')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Enemies.Bosses')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.HUD')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Items')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Objects')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Triggers')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Weapons')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Sound')");
        lua.DoString(@"import('XSharp', 'XSharp.Engine.World')");
        lua["engine"] = this;

        base.Initialize((object) initializers);
    }

    protected override void Unload()
    {
        DisposeResource(lua);
        DisposeResource(Direct3D);

        base.Unload();
    }

    protected override void DisposeDeviceResources()
    {
        for (int i = 0; i < renderTargets.Length; i++)
        {
            var target = renderTargets[i];
            DisposeResource(target);
            renderTargets[i] = null;
        }

        DisposeResource(PixelShader);
        DisposeResource(PaletteShader);
        DisposeResource(sprite);
        DisposeResource(VertexBuffer);
        DisposeResource(Device);

        Device = null;
    }

    public override DataStream CreateDataStream(IntPtr ptr, int sizeInBytes, bool canRead, bool canWrite)
    {
        return new DX9DataStream(ptr, sizeInBytes, canRead, canWrite);
    }

    public override ITexture CreateEmptyTexture(int width, int height, Format format = Format.L8)
    {
        return CreateEmptyTexture(width, height, Usage.None, format, Pool.Managed);
    }

    public DX9Texture CreateEmptyTexture(int width, int height, Usage usage, Format format, Pool pool)
    {
        return new DX9Texture(Device, width, height, 1, usage, format, pool);
    }

    public override ITexture CreateImageTextureFromFile(string filePath, bool systemMemory = true)
    {
        return CreateImageTextureFromFile(filePath, Usage.None, systemMemory ? Pool.SystemMemory : Pool.Default);
    }

    public DX9Texture CreateImageTextureFromFile(string filePath, Usage usage, Pool pool)
    {
        var result = Texture.FromFile(Device, filePath, usage, pool);
        return new DX9Texture(result);
    }

    public DX9Texture CreateImageTextureFromEmbeddedResource(string path, Usage usage = Usage.None, Pool pool = Pool.SystemMemory)
    {
        return CreateImageTextureFromEmbeddedResource(Assembly.GetExecutingAssembly(), path, usage, pool);
    }

    public DX9Texture CreateImageTextureFromEmbeddedResource(Assembly assembly, string path, Usage usage = Usage.None, Pool pool = Pool.SystemMemory)
    {
        string assemblyName = assembly.GetName().Name;
        using var stream = assembly.GetManifestResourceStream($"{assemblyName}.Assets.{path}");
        var texture = CreateImageTextureFromStream(stream, usage, pool);
        return texture;
    }

    public override ITexture CreateImageTextureFromStream(Stream stream, bool systemMemory = true)
    {
        return CreateImageTextureFromStream(stream, Usage.None, systemMemory ? Pool.SystemMemory : Pool.Default);
    }

    public DX9Texture CreateImageTextureFromStream(Stream stream, Usage usage, Pool pool)
    {
        var result = Texture.FromStream(Device, stream, usage, pool);
        return new DX9Texture(result);
    }

    protected override IFont CreateFont(FontDescription description)
    {
        return new DX9Font(sprite, new Font(Device, description.ToDX9FontDescription()));
    }

    protected override ILine CreateLine()
    {
        return new DX9Line(new Line(Device));
    }

    public override void RenderSprite(ITexture texture, Palette palette, FadingControl fadingControl, Box box, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RectangleF rDest = WorldBoxToScreen(box, false);

        sprite.Begin(SpriteFlags.AlphaBlend);

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);

        PixelShader shader;
        EffectHandle fadingLevelHandle;
        EffectHandle fadingColorHandle;

        if (palette != null)
        {
            fadingLevelHandle = plsFadingLevelHandle;
            fadingColorHandle = plsFadingColorHandle;
            shader = PaletteShader;
            Device.SetTexture(1, (DX9Texture) palette.Texture);
        }
        else
        {
            fadingLevelHandle = psFadingLevelHandle;
            fadingColorHandle = psFadingColorHandle;
            shader = PixelShader;
        }

        Device.PixelShader = shader;
        Device.VertexShader = null;

        if (fadingControl != null)
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, fadingControl.FadingLevel);
            shader.Function.ConstantTable.SetValue(Device, fadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, Vector4.Zero);
        }

        var matTranslation = DX9Matrix.Translation(rDest.Left, rDest.Top, 0);
        DX9Matrix matTransform = matTranslation * transform.ToDX9Matrix();
        sprite.Transform = matTransform;

        if (repeatX > 1 || repeatY > 1)
        {
            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);

            sprite.Draw((DX9Texture) texture, Color.FromRgba(0xffffffff).ToDX9Color(), (box.Scale(box.Origin, repeatX, repeatY) - box.Origin).ToRectangleF().ToDX9RectangleF());
        }
        else
        {
            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

            sprite.Draw((DX9Texture) texture, Color.FromRgba(0xffffffff).ToDX9Color());
        }

        sprite.End();
    }

    public override void WriteVertex(DataStream vbData, float x, float y, float u, float v)
    {
        vbData.Write(x);
        vbData.Write(y);
        vbData.Write(0f);
        vbData.Write(0xffffffff);
        vbData.Write(u);
        vbData.Write(v);
    }

    public void SetupQuad(VertexBuffer vb, float width, float height)
    {
        DX9DataStream vbData = vb.Lock(0, 0, D3D9LockFlags.None);
        WriteSquare(vbData, (0, 0), (0, 0), (1, 1), (width, height));
        vb.Unlock();
    }

    public void SetupQuad(VertexBuffer vb)
    {
        DX9DataStream vbData = vb.Lock(0, 4 * VERTEX_SIZE, D3D9LockFlags.None);

        WriteVertex(vbData, 0, 0, 0, 0);
        WriteVertex(vbData, 1, 0, 1, 0);
        WriteVertex(vbData, 1, -1, 1, 1);
        WriteVertex(vbData, 0, -1, 0, 1);

        vb.Unlock();
    }

    public void RenderVertexBuffer(VertexBuffer vb, int vertexSize, int primitiveCount, ITexture texture, Palette palette, FadingControl fadingControl, Box box)
    {
        Device.SetStreamSource(0, vb, 0, vertexSize);

        RectangleF rDest = WorldBoxToScreen(box, false);

        var drawScale = GetDrawScale();
        float x = rDest.Left * (float) drawScale.X - (float) StageSize.X * 0.5f;
        float y = -rDest.Top * (float) drawScale.Y + (float) StageSize.Y * 0.5f;

        var matScaling = DX9Matrix.Scaling((float) drawScale.X, (float) drawScale.Y, 1);
        var matTranslation = DX9Matrix.Translation(x, y, 0);
        DX9Matrix matTransform = matScaling * matTranslation;

        Device.SetTransform(TransformState.World, matTransform);
        Device.SetTransform(TransformState.View, DX9Matrix.Identity);
        Device.SetTransform(TransformState.Texture0, DX9Matrix.Identity);
        Device.SetTransform(TransformState.Texture1, DX9Matrix.Identity);
        Device.SetTexture(0, (DX9Texture) texture);

        PixelShader shader;
        EffectHandle fadingLevelHandle;
        EffectHandle fadingColorHandle;

        if (palette != null)
        {
            fadingLevelHandle = plsFadingLevelHandle;
            fadingColorHandle = plsFadingColorHandle;
            shader = PaletteShader;
            Device.SetTexture(1, (DX9Texture) palette.Texture);
        }
        else
        {
            fadingLevelHandle = psFadingLevelHandle;
            fadingColorHandle = psFadingColorHandle;
            shader = PixelShader;
        }

        Device.PixelShader = shader;
        Device.VertexShader = null;

        if (fadingControl != null)
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, fadingControl.FadingLevel);
            shader.Function.ConstantTable.SetValue(Device, fadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, Vector4.Zero);
        }

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        Device.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
    }

    protected override SpriteSheet CreateSpriteSheet(bool disposeTexture = false, bool precache = false)
    {
        return new DX9SpriteSheet(disposeTexture, precache);
    }

    protected override SpriteSheet CreateSpriteSheet(ITexture texture, bool disposeTexture = false, bool precache = false)
    {
        return new DX9SpriteSheet(texture, disposeTexture, precache);
    }

    protected override SpriteSheet CreateSpriteSheet(string imageFileName, bool precache = false)
    {
        return new DX9SpriteSheet(imageFileName, precache);
    }

    protected override Palette CreatePalette(ITexture texture, int index, string name, int count)
    {
        return new DX9Palette((DX9Texture) texture, index, name, count);
    }

    protected override Scene CreateScene(int id)
    {
        return new DX9Scene(id);
    }

    protected override bool BeginScene()
    {
        if (Device == null)
            return false;

        var hr = Device.TestCooperativeLevel();
        if (hr.Success)
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor.ToDX9Color(), 1.0f, 0);
            Device.BeginScene();
            return true;
        }

        if (hr == ResultCode.DeviceLost)
            return false;

        if (hr == ResultCode.DeviceNotReset)
        {
            ResetDevice();
            return false;
        }

        Running = false;
        throw new Exception($"Exiting process due device error: {hr} ({hr.Code})");
    }

    protected override void EndScene()
    {
        Device.EndScene();
    }

    protected override void Present()
    {
        Device.Present();
    }

    public override bool IsFocused()
    {
        return Control.Focused;
    }

    protected override Point GetCursorPosition()
    {
        return Cursor.Position.ToPoint();
    }

    public override Point PointToClient(Point point)
    {
        return Control.PointToClient(point.ToSDPoint()).ToPoint();
    }

    protected override IRenderTarget GetRenderTarget(int level)
    {
        var result = renderTargets[level];
        if (result != null)
            return result;

        var surface = Device.GetRenderTarget(level);
        if (surface == null)
            return null;

        result = new DX9RenderTarget(Device.GetRenderTarget(level), false);
        renderTargets[level] = result;
        return result;
    }

    protected override void SetRenderTarget(int level, IRenderTarget target)
    {
        Device.SetRenderTarget(0, (DX9RenderTarget) target);
        renderTargets[level] = target;
    }

    protected override void Clear(Color color)
    {
        Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, color.ToDX9Color(), 1.0f, 0);
    }

    protected override void PrepareRender()
    {
        var orthoLH = DX9Matrix.OrthoLH((float) StageSize.X, (float) StageSize.Y, 0.0f, 1.0f);
        Device.SetTransform(TransformState.Projection, orthoLH);
        Device.SetTransform(TransformState.World, DX9Matrix.Identity);
        Device.SetTransform(TransformState.View, DX9Matrix.Identity);
    }

    protected override void DoRenderLoop(Action action)
    {
        RenderLoop.Run(Control, () => action());
    }

    public override void DrawLine(Vector2 from, Vector2 to, float width, Color color, FadingControl fadingControl = null)
    {
        //from *= 4;
        //to *= 4;

        line.Width = width;

        line.Begin();

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        line.Draw(from, to, color);
        line.End();
    }

    public override void DrawRectangle(RectangleF rect, float borderWith, Color color, FadingControl fadingControl = null)
    {
        var scale = GetCameraScale();
        float scaleX = (float) scale.X;
        float scaleY = (float) scale.Y;

        rect = new RectangleF(rect.X, rect.Y + 1, rect.Width, rect.Height);

        line.Width = borderWith;

        line.Begin();

        var matScaling = DX9Matrix.Scaling(1, 1, 1);
        var matTranslation = DX9Matrix.Translation(0, 0, 0);
        DX9Matrix matTransform = matScaling * matTranslation;

        Device.SetTransform(TransformState.World, matTransform);
        Device.SetTransform(TransformState.View, DX9Matrix.Identity);

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        line.Draw(
            [
                rect.TopLeft,
                rect.TopRight,
                rect.BottomRight,
                rect.BottomLeft,
                rect.TopLeft
            ],
            color);
        line.End();
    }

    public override void FillRectangle(RectangleF rect, Color color, FadingControl fadingControl = null)
    {
        float x = rect.Left;
        float y = rect.Top;

        var matScaling = DX9Matrix.Scaling(rect.Width, rect.Height, 1);
        var matTranslation = DX9Matrix.Translation(x, y + 1, 0);
        DX9Matrix matTransform = matScaling * matTranslation;

        sprite.Begin(SpriteFlags.AlphaBlend);

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        Device.VertexShader = null;

        sprite.Transform = matTransform;

        sprite.Draw((DX9Texture) (color == Color.Black ? blackPixelTexture : whitePixelTexture), color.ToDX9Color());
        sprite.End();
    }

    public override void DrawText(string text, IFont font, RectangleF drawRect, FontDrawFlags drawFlags, Matrix transform, Color color, out RectangleF fontDimension, FadingControl fadingControl = null)
    {
        sprite.Begin();

        Device.VertexShader = null;
        Device.PixelShader = null;

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        Device.VertexShader = null;

        sprite.Transform = transform.ToDX9Matrix();

        fontDimension = font.MeasureText(text, drawRect, drawFlags);
        font.DrawText(text, fontDimension, drawFlags, color);
        sprite.End();
    }

    public override void DrawTexture(Box destBox, ITexture texture, Palette palette = null, bool linear = false)
    {
        Device.PixelShader = PixelShader;
        Device.VertexShader = null;

        PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, FadingControl.FadingLevel);
        PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, FadingControl.FadingColor.ToVector4());

        var matScaling = DX9Matrix.Scaling(1, 1, 1);
        var matTranslation = DX9Matrix.Translation(-1 * (float) destBox.Width * 0.5F, +1 * (float) destBox.Height * 0.5F, 1);
        DX9Matrix matTransform = matScaling * matTranslation;

        Device.SetTransform(TransformState.World, matTransform);
        Device.SetTransform(TransformState.View, DX9Matrix.Identity);
        Device.SetTransform(TransformState.Texture0, DX9Matrix.Identity);
        Device.SetTransform(TransformState.Texture1, DX9Matrix.Identity);

        PixelShader shader;

        if (palette != null)
        {
            shader = PaletteShader;
            Device.SetTexture(1, (DX9Texture) palette.Texture);
        }
        else
        {
            shader = PixelShader;
        }

        Device.PixelShader = shader;
        Device.VertexShader = null;

        Device.SetSamplerState(0, SamplerState.MagFilter, linear ? TextureFilter.Linear : TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MinFilter, linear ? TextureFilter.Linear : TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        Device.SetStreamSource(0, VertexBuffer, 0, VERTEX_SIZE);
        Device.SetTexture(0, (DX9Texture) texture);
        Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
    }

    public override void ShowErrorMessage(string message)
    {
        MessageBox.Show(message);
    }

    public void LoadConfig()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
            config.Save();
        }

        if (Control is Form)
        {
            if (section.Left != -1)
                Control.Left = section.Left;

            if (section.Top != -1)
                Control.Top = section.Top;
        }

        drawHitbox = section.DrawCollisionBox;
        showColliders = section.ShowColliders;
        drawLevelBounds = section.DrawMapBounds;
        drawTouchingMapBounds = section.DrawTouchingMapBounds;
        drawHighlightedPointingTiles = section.DrawHighlightedPointingTiles;
        drawPlayerOriginAxis = section.DrawPlayerOriginAxis;
        showInfoText = section.ShowInfoText;
        showCheckpointBounds = section.ShowCheckpointBounds;
        showTriggerBounds = section.ShowTriggerBounds;
        showTriggerCameraLockDirection = section.ShowTriggerCameraLook;
        CurrentSaveSlot = section.CurrentSaveSlot;
    }

    public void SaveConfig()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
        }

        if (Control is Form)
        {
            section.Left = Control.Left;
            section.Top = Control.Top;
        }

        section.DrawCollisionBox = drawHitbox;
        section.ShowColliders = showColliders;
        section.DrawMapBounds = drawLevelBounds;
        section.DrawTouchingMapBounds = drawTouchingMapBounds;
        section.DrawHighlightedPointingTiles = drawHighlightedPointingTiles;
        section.DrawPlayerOriginAxis = drawPlayerOriginAxis;
        section.ShowInfoText = showInfoText;
        section.ShowCheckpointBounds = showCheckpointBounds;
        section.ShowTriggerBounds = showTriggerBounds;
        section.ShowTriggerCameraLook = showTriggerCameraLockDirection;
        section.CurrentSaveSlot = CurrentSaveSlot;

        config.Save();
    }

    protected override SoundChannel CreateSoundChannel(float volume = 1)
    {
        return new NAudioSoundChannel(volume);
    }
}