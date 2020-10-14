#if 0
//
// Generated by Microsoft (R) HLSL Shader Compiler 10.1
//
// Parameters:
//
//   sampler2D image;
//   sampler1D palette;
//
//
// Registers:
//
//   Name         Reg   Size
//   ------------ ----- ----
//   image        s0       1
//   palette      s1       1
//

    ps_2_0
    def c0, 15.9375, 0.03125, 0, 0
    dcl t0.xy
    dcl_2d s0
    dcl_2d s1
    texld r0, t0, s0
    mad r0.xy, r0.x, c0.x, c0.y
    texld r0, r0, s1
    mov oC0, r0

// approximately 4 instruction slots used (2 texture, 2 arithmetic)
#endif

const BYTE g_ps20_main[] =
{
      0,   2, 255, 255, 254, 255, 
     42,   0,  67,  84,  65,  66, 
     28,   0,   0,   0, 123,   0, 
      0,   0,   0,   2, 255, 255, 
      2,   0,   0,   0,  28,   0, 
      0,   0,   0,   1,   0,   0, 
    116,   0,   0,   0,  68,   0, 
      0,   0,   3,   0,   0,   0, 
      1,   0,   0,   0,  76,   0, 
      0,   0,   0,   0,   0,   0, 
     92,   0,   0,   0,   3,   0, 
      1,   0,   1,   0,   0,   0, 
    100,   0,   0,   0,   0,   0, 
      0,   0, 105, 109,  97, 103, 
    101,   0, 171, 171,   4,   0, 
     12,   0,   1,   0,   1,   0, 
      1,   0,   0,   0,   0,   0, 
      0,   0, 112,  97, 108, 101, 
    116, 116, 101,   0,   4,   0, 
     11,   0,   1,   0,   1,   0, 
      1,   0,   0,   0,   0,   0, 
      0,   0, 112, 115,  95,  50, 
     95,  48,   0,  77, 105,  99, 
    114, 111, 115, 111, 102, 116, 
     32,  40,  82,  41,  32,  72, 
     76,  83,  76,  32,  83, 104, 
     97, 100, 101, 114,  32,  67, 
    111, 109, 112, 105, 108, 101, 
    114,  32,  49,  48,  46,  49, 
      0, 171,  81,   0,   0,   5, 
      0,   0,  15, 160,   0,   0, 
    127,  65,   0,   0,   0,  61, 
      0,   0,   0,   0,   0,   0, 
      0,   0,  31,   0,   0,   2, 
      0,   0,   0, 128,   0,   0, 
      3, 176,  31,   0,   0,   2, 
      0,   0,   0, 144,   0,   8, 
     15, 160,  31,   0,   0,   2, 
      0,   0,   0, 144,   1,   8, 
     15, 160,  66,   0,   0,   3, 
      0,   0,  15, 128,   0,   0, 
    228, 176,   0,   8, 228, 160, 
      4,   0,   0,   4,   0,   0, 
      3, 128,   0,   0,   0, 128, 
      0,   0,   0, 160,   0,   0, 
     85, 160,  66,   0,   0,   3, 
      0,   0,  15, 128,   0,   0, 
    228, 128,   1,   8, 228, 160, 
      1,   0,   0,   2,   0,   8, 
     15, 128,   0,   0, 228, 128, 
    255, 255,   0,   0
};
