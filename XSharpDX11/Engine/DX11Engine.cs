using NAudio.CoreAudioApi;
using NLua;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XSharp.Engine;
using XSharp.Engine.Graphics;
using XSharp.Engine.Input;
using XSharp.Engine.Sound;
using XSharp.Engine.World;
using XSharp.Graphics;
using XSharp.Interop;
using XSharpDX11.Engine.Graphics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static XSharp.Engine.Consts;
using Box = XSharp.Math.Fixed.Geometry.Box;
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = XSharp.Graphics.Color;
using Configuration = System.Configuration.Configuration;
using D3D9LockFlags = SharpDX.Direct3D11.LockFlags;
using DataRectangle = XSharp.Graphics.DataRectangle;
using DataStream = XSharp.Graphics.DataStream;
using Device = SharpDX.Direct3D11.Device;
using Device9 = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using DeviceType = SharpDX.Direct3D11.DeviceType;
using DX11FontDescription = SharpDX.Direct3D11.FontDescription;
using DX11Format = SharpDX.Direct3D11.Format;
using DX9Color = SharpDX.Color;
using DX9Matrix = SharpDX.Matrix;
using DX9RectangleF = SharpDX.RectangleF;
using DX9Vector2 = SharpDX.Vector2;
using DXSprite = SharpDX.Direct3D11.Sprite;
using Font = SharpDX.Direct3D11.Font;
using FontDescription = XSharp.Graphics.FontDescription;
using FontDrawFlags = XSharp.Graphics.FontDrawFlags;
using Format = XSharp.Graphics.Format;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Matrix = XSharp.Math.Geometry.Matrix;
using Point = XSharp.Math.Geometry.Point;
using PresentFlags = SharpDX.Direct3D11.PresentFlags;
using PresentParameters = SharpDX.Direct3D11.PresentParameters;
using Rectangle = SharpDX.Rectangle;
using RectangleF = XSharp.Math.Geometry.RectangleF;
using ResultCode = SharpDX.Direct3D11.ResultCode;
using Size2F = XSharp.Math.Geometry.Size2F;
using SwapEffect = SharpDX.Direct3D11.SwapEffect;
using Usage = SharpDX.Direct3D11.Usage;
using Vector2 = XSharp.Math.Geometry.Vector2;
using Vector4 = SharpDX.Vector4;

namespace XSharp.Engine;

public class DX11Engine : BaseEngine
{
    private static float[] QUAD_VERTICES =
    [
      // position     uv
        -1,  1, 0,    0, 0,
         1,  1, 0,    1, 0,
         1, -1, 0,    1, 1,

        -1,  1, 0,    0, 0,
         1, -1, 0,    1, 1,
        -1, -1, 0,    0, 1
    ];
    new public static DX11Engine Engine => (DX11Engine) BaseEngine.Engine;

    public const int VERTEX_FIELD_COUNT = 5;
    public const int VERTEX_SIZE = VERTEX_FIELD_COUNT * sizeof(float);

    private PresentParameters presentationParams;
    private DXSprite sprite;

    private Buffer vsTransformBuffer;
    private Buffer psFadingParams;
    private Buffer plsFadingParams;

    private DirectInput directInput;

    private IRenderTarget[] renderTargets = new IRenderTarget[16];

    private Lua lua;

    public Control Control
    {
        get;
        private set;
    }

    public Device Device
    {
        get;
        private set;
    }

    public DeviceContext Context => Device.ImmediateContext;

    public SwapChain SwapChain
    {
        get;
        private set;
    }

    public VertexBufferBinding VertexBuffer
    {
        get;
        private set;
    }
    public VertexShader VertexShader
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

    public SamplerState Sampler
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
        return new DX11Texture(Device, (int) StageSize.X, (int) StageSize.Y, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
    }

    protected override void InitGraphicDevice()
    {
        // Creates the Device
        var swapChainDesc = new SwapChainDescription()
        {
            BufferCount = DOUBLE_BUFFERED ? 2 : 1,
            ModeDescription = new ModeDescription(Control.ClientSize.Width, Control.ClientSize.Height,
                new Rational(TICKRATE, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
            IsWindowed = !FULL_SCREEN,
            OutputHandle = Control.Handle,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.Discard,
            Usage = Usage.RenderTargetOutput
        };

        SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport,
            swapChainDesc, out var device, out var swapChain);
        Device = device;
        SwapChain = swapChain;

        // Render target view
        using var backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
        using var renderTargetView = new RenderTargetView(device, backBuffer);

        // Viewport
        Context.Rasterizer.SetViewport(0, 0, Control.ClientSize.Width, Control.ClientSize.Height);

        var vertexShaderByteCode = ShaderBytecode.CompileFromFile("VertexShader.hlsl", "VSMain", "ps_4_0");
        VertexShader = new VertexShader(device, vertexShaderByteCode);

        // Cria buffer
        var bufferDesc = new BufferDescription()
        {
            Usage = ResourceUsage.Default,
            SizeInBytes = Utilities.SizeOf<Matrix>(),
            BindFlags = BindFlags.ConstantBuffer,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None,
        };

        // Cria um DataStream para a matriz
        var dataStream = new DX11DataStream(Utilities.SizeOf<Matrix>(), true, true);
        dataStream.Write(Matrix.Identity);
        dataStream.Position = 0;

        vsTransformBuffer = new Buffer(device, dataStream, bufferDesc);

        var pixelShaderBytecode = ShaderBytecode.CompileFromFile("PixelShader.hlsl", "PSMain", "ps_4_0");
        PixelShader = new PixelShader(device, pixelShaderBytecode);

        bufferDesc = new BufferDescription()
        {
            Usage = ResourceUsage.Default,
            SizeInBytes = Utilities.SizeOf<FadingParams>(),
            BindFlags = BindFlags.ConstantBuffer,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None,
        };

        // Cria um DataStream para a matriz
        dataStream = new DX11DataStream(Utilities.SizeOf<FadingParams>(), true, true);
        dataStream.Write(new FadingParams());
        dataStream.Position = 0;

        psFadingParams = new Buffer(device, dataStream, bufferDesc);

        bufferDesc = new BufferDescription()
        {
            Usage = ResourceUsage.Default,
            SizeInBytes = Utilities.SizeOf<FadingParams>(),
            BindFlags = BindFlags.ConstantBuffer,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None,
        };

        // Cria um DataStream para a matriz
        dataStream = new DX11DataStream(Utilities.SizeOf<FadingParams>(), true, true);
        dataStream.Write(new FadingParams());
        dataStream.Position = 0;

        vsTransformBuffer = new Buffer(device, dataStream, bufferDesc);

        pixelShaderBytecode = ShaderBytecode.CompileFromFile("PaletteShader.hlsl", "PSMain", "ps_4_0");
        PaletteShader = new PixelShader(device, pixelShaderBytecode);

        plsFadingParams = new Buffer(device, dataStream, bufferDesc);

        // Vertex layout
        var layout = new InputLayout(device,
            ShaderSignature.GetInputSignature(vertexShaderByteCode),
            new[] {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 3 * sizeof(float), 0)
            });

        Context.InputAssembler.InputLayout = layout;
        Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        var samplerDesc = new SamplerStateDescription()
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            ComparisonFunction = Comparison.Never,
            MinimumLod = 0,
            MaximumLod = float.MaxValue
        };
        Sampler = new SamplerState(device, samplerDesc);

        Context.VertexShader.Set(VertexShader);
        Context.PixelShader.Set(PixelShader);
        Context.PixelShader.SetSampler(0, Sampler);
        
        var buffer = Buffer.Create(device, BindFlags.VertexBuffer, QUAD_VERTICES);
        VertexBuffer = new VertexBufferBinding(buffer, VERTEX_SIZE, 0);

        SetupQuad(buffer, (float) StageSize.X, (float) StageSize.Y);

        sprite = new DXSprite(device);
    }

    protected override IKeyboard CreateKeyboard()
    {
        var result = new Keyboard(directInput);
        result.Properties.BufferSize = 2048;
        result.Acquire();
        return new DX11Keyboard(result);
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
            return new DX11Joystick(result);
        }

        return null;
    }

    protected override void Initialize(dynamic initializers)
    {
        Control = initializers.control;

        lua = new Lua();

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
        DisposeResource(PixelShader);
        DisposeResource(PaletteShader);
        DisposeResource(sprite);               
        DisposeResource(VertexBuffer);
        DisposeResource(Device);

        Device = null;
    }

    public override DataStream CreateDataStream(IntPtr ptr, int sizeInBytes, bool canRead, bool canWrite)
    {
        return new DX11DataStream(ptr, sizeInBytes, canRead, canWrite);
    }

    public override ITexture CreateEmptyTexture(int width, int height, Format format = Format.L8)
    {
        return CreateEmptyTexture(width, height, Usage.None, format, Pool.Managed);
    }

    public DX11Texture CreateEmptyTexture(int width, int height, Usage usage, Format format, Pool pool)
    {
        return new DX11Texture(Device, width, height, 1, usage, format, pool);
    }

    public override ITexture CreateImageTextureFromFile(string filePath, bool systemMemory = true)
    {
        return CreateImageTextureFromFile(filePath, Usage.None, systemMemory ? Pool.SystemMemory : Pool.Default);
    }

    public DX11Texture CreateImageTextureFromFile(string filePath, Usage usage, Pool pool)
    {
        var result = Texture2D.FromFile(Device, filePath, usage, pool);
        return new DX11Texture(result);
    }

    public DX11Texture CreateImageTextureFromEmbeddedResource(string path, Usage usage = Usage.None, Pool pool = Pool.SystemMemory)
    {
        return CreateImageTextureFromEmbeddedResource(Assembly.GetExecutingAssembly(), path, usage, pool);
    }

    public DX11Texture CreateImageTextureFromEmbeddedResource(Assembly assembly, string path, Usage usage = Usage.None, Pool pool = Pool.SystemMemory)
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

    public DX11Texture CreateImageTextureFromStream(Stream stream, Usage usage, Pool pool)
    {
        var result = Texture2D.FromStream(Device, stream, usage, pool);
        return new DX11Texture(result);
    }

    protected override IFont CreateFont(FontDescription description)
    {
        return new DX11Font(sprite, new Font(Device, description.ToDX9FontDescription()));
    }

    protected override ILine CreateLine()
    {
        return new DX11Line(new Line(Device));
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
            fadingLevelHandle = plsFadingParams;
            fadingColorHandle = plsFadingColorHandle;
            shader = PaletteShader;
            Device.SetTexture(1, (DX9Texture) palette.Texture);
        }
        else
        {
            fadingLevelHandle = psFadingParams;
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

            sprite.Draw((DX11Texture) texture, Color.FromRgba(0xffffffff).ToDX9Color(), (box.Scale(box.Origin, repeatX, repeatY) - box.Origin).ToRectangleF().ToDX9RectangleF());
        }
        else
        {
            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

            sprite.Draw((DX11Texture) texture, Color.FromRgba(0xffffffff).ToDX9Color());
        }

        sprite.End();
    }

    public override void WriteVertex(DataStream vbData, float x, float y, float u, float v)
    {
        vbData.Write(x);
        vbData.Write(y);
        vbData.Write(0f);
        vbData.Write(u);
        vbData.Write(v);
    }

    public void SetupQuad(DataStream vbData, float width, float height)
    {
        WriteSquare(vbData, (0, 0), (0, 0), (1, 1), (width, height));
    }

    public void SetupQuad(Buffer vb)
    {
        var box = Context.MapSubresource(
            vb,
            0,
            MapMode.WriteDiscard,
            MapFlags.None
        );
        try
        {
            DX11DataStream vbData = new DX11DataStream(box.DataPointer, 4 * VERTEX_SIZE, true, true);

            WriteVertex(vbData, 0, 0, 0, 0);
            WriteVertex(vbData, 1, 0, 1, 0);
            WriteVertex(vbData, 1, -1, 1, 1);
            WriteVertex(vbData, 0, -1, 0, 1);
        }
        finally
        {
            Context.UnmapSubresource(vb, 0);
        }
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
            fadingLevelHandle = plsFadingParams;
            fadingColorHandle = plsFadingColorHandle;
            shader = PaletteShader;
            Device.SetTexture(1, (DX11Texture) palette.Texture);
        }
        else
        {
            fadingLevelHandle = psFadingParams;
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
        return new DX11SpriteSheet(disposeTexture, precache);
    }

    protected override SpriteSheet CreateSpriteSheet(ITexture texture, bool disposeTexture = false, bool precache = false)
    {
        return new DX11SpriteSheet(texture, disposeTexture, precache);
    }

    protected override SpriteSheet CreateSpriteSheet(string imageFileName, bool precache = false)
    {
        return new DX11SpriteSheet(imageFileName, precache);
    }

    protected override Palette CreatePalette(ITexture texture, int index, string name, int count)
    {
        return new DX11Palette((DX11Texture) texture, index, name, count);
    }

    protected override Scene CreateScene(int id)
    {
        return new DX11Scene(id);
    }

    protected override bool BeginScene()
    {
        if (Device == null)
            return false;

        var hr = Device.TestCooperativeLevel();
        if (hr.Success)
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor.ToDX11Color(), 1.0f, 0);
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

        result  = new DX11RenderTarget(Device.GetRenderTarget(level));
        renderTargets[level] = result;
        return result;
    }

    protected override void SetRenderTarget(int level, IRenderTarget target)
    {
        Device.SetRenderTarget(0, (DX11RenderTarget) target);
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
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingParams, fadingControl.FadingLevel);
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
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingParams, fadingControl.FadingLevel);
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
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingParams, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        Device.VertexShader = null;

        sprite.Transform = matTransform;

        sprite.Draw((DX11Texture) (color == Color.Black ? blackPixelTexture : whitePixelTexture), color.ToDX9Color());
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
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingParams, fadingControl.FadingLevel);
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

        PixelShader.Function.ConstantTable.SetValue(Device, psFadingParams, FadingControl.FadingLevel);
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
            Device.SetTexture(1, (DX11Texture) palette.Texture);
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
        Device.SetTexture(0, (DX11Texture) texture);
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