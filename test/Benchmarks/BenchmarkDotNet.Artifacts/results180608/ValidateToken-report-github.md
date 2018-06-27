``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 7 SP1 (6.1.7601.0)
Intel Core i7-5600U CPU 2.60GHz (Broadwell), 1 CPU, 4 logical and 2 physical cores
Frequency=2533242 Hz, Resolution=394.7511 ns, Timer=TSC
.NET Core SDK=2.1.300
  [Host]     : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Job-FTEDFH : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Runtime=Core  Server=True  Toolchain=.NET Core 2.1  
RunStrategy=Throughput  

```
|     Method |      token |         Mean |       Error |      StdDev |      Op/s | Scaled | ScaledSD |     Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|----------- |----------- |-------------:|------------:|------------:|----------:|-------:|---------:|----------:|----------:|----------:|----------:|
|        **Jwt** |        **big** |  **1,489.61 us** |  **27.9716 us** |  **26.1646 us** |    **671.31** |   **1.00** |     **0.00** |  **177.7344** |  **177.7344** |  **177.7344** |  **878266 B** |
|     Wilson |        big |  4,779.40 us | 113.9133 us | 144.0638 us |    209.23 |   3.21 |     0.11 |  492.1875 |  492.1875 |  492.1875 | 2024028 B |
| JoseDotNet |        big |  2,776.33 us |  16.1070 us |  14.2784 us |    360.19 |   1.86 |     0.03 |  500.0000 |  496.0938 |  496.0938 | 2036276 B |
|  JwtDotNet |        big |  3,428.14 us |  41.1390 us |  36.4686 us |    291.70 |   2.30 |     0.05 |  578.1250 |  574.2188 |  574.2188 | 2751454 B |
|            |            |              |             |             |           |        |          |           |           |           |           |
|        **Jwt** |      **empty** |     **10.19 us** |   **0.0492 us** |   **0.0384 us** | **98,105.31** |   **1.00** |     **0.00** |    **0.2594** |    **0.0153** |         **-** |    **6784 B** |
|     Wilson |      empty |     17.69 us |   0.0851 us |   0.0755 us | 56,539.67 |   1.74 |     0.01 |    0.3967 |    0.0305 |         - |   10576 B |
| JoseDotNet |      empty |     13.00 us |   0.0649 us |   0.0507 us | 76,924.76 |   1.28 |     0.01 |    0.4272 |    0.0305 |         - |   10696 B |
|  JwtDotNet |      empty |     15.21 us |   0.0410 us |   0.0320 us | 65,740.44 |   1.49 |     0.01 |    0.5188 |    0.0305 |         - |   12728 B |
|            |            |              |             |             |           |        |          |           |           |           |           |
|        **Jwt** |    **enc-big** |  **3,019.77 us** |  **64.2576 us** | **117.4986 us** |    **331.15** |   **1.00** |     **0.00** |  **328.1250** |  **328.1250** |  **328.1250** | **1714005 B** |
|     Wilson |    enc-big | 10,826.78 us | 223.0759 us | 197.7510 us |     92.36 |   3.59 |     0.15 | 1203.1250 | 1203.1250 | 1203.1250 | 4497543 B |
| JoseDotNet |    enc-big |           NA |          NA |          NA |        NA |      ? |        ? |       N/A |       N/A |       N/A |       N/A |
|            |            |              |             |             |           |        |          |           |           |           |           |
|        **Jwt** |  **enc-empty** |     **58.59 us** |   **1.1181 us** |   **1.0459 us** | **17,068.35** |   **1.00** |     **0.00** |    **1.0376** |    **0.1831** |         **-** |   **25552 B** |
|     Wilson |  enc-empty |     61.91 us |   0.7327 us |   0.6118 us | 16,151.66 |   1.06 |     0.02 |    1.2207 |    0.1221 |         - |   32000 B |
| JoseDotNet |  enc-empty |           NA |          NA |          NA |        NA |      ? |        ? |       N/A |       N/A |       N/A |       N/A |
|            |            |              |             |             |           |        |          |           |           |           |           |
|        **Jwt** | **enc-medium** |     **85.74 us** |   **1.0243 us** |   **0.9080 us** | **11,663.52** |   **1.00** |     **0.00** |    **1.4648** |    **0.1221** |         **-** |   **37184 B** |
|     Wilson | enc-medium |    133.40 us |   0.9321 us |   0.7783 us |  7,496.06 |   1.56 |     0.02 |    2.6855 |    0.2441 |         - |   69193 B |
| JoseDotNet | enc-medium |           NA |          NA |          NA |        NA |      ? |        ? |       N/A |       N/A |       N/A |       N/A |
|            |            |              |             |             |           |        |          |           |           |           |           |
|        **Jwt** |  **enc-small** |     **67.48 us** |   **0.9667 us** |   **0.9042 us** | **14,819.28** |   **1.00** |     **0.00** |    **1.0986** |    **0.1221** |         **-** |   **27840 B** |
|     Wilson |  enc-small |     83.19 us |   0.3238 us |   0.2871 us | 12,020.88 |   1.23 |     0.02 |    1.7090 |    0.1221 |         - |   42673 B |
| JoseDotNet |  enc-small |           NA |          NA |          NA |        NA |      ? |        ? |       N/A |       N/A |       N/A |       N/A |
|            |            |              |             |             |           |        |          |           |           |           |           |
|        **Jwt** |     **medium** |     **29.61 us** |   **0.0882 us** |   **0.0689 us** | **33,777.33** |   **1.00** |     **0.00** |    **0.5493** |    **0.0305** |         **-** |   **13176 B** |
|     Wilson |     medium |     56.90 us |   1.0670 us |   1.0479 us | 17,575.86 |   1.92 |     0.03 |    1.2207 |         - |         - |   31680 B |
| JoseDotNet |     medium |     32.33 us |   0.2828 us |   0.2507 us | 30,931.57 |   1.09 |     0.01 |    0.9766 |         - |         - |   24640 B |
|  JwtDotNet |     medium |     64.12 us |   0.2002 us |   0.1672 us | 15,594.59 |   2.17 |     0.01 |    1.7090 |         - |         - |   44481 B |
|            |            |              |             |             |           |        |          |           |           |           |           |
|        **Jwt** |      **small** |     **16.20 us** |   **0.0621 us** |   **0.0518 us** | **61,743.41** |   **1.00** |     **0.00** |    **0.3052** |    **0.0305** |         **-** |    **7832 B** |
|     Wilson |      small |     30.10 us |   0.1224 us |   0.1085 us | 33,221.06 |   1.86 |     0.01 |    0.6714 |    0.0305 |         - |   17336 B |
| JoseDotNet |      small |     18.62 us |   0.0676 us |   0.0528 us | 53,692.35 |   1.15 |     0.00 |    0.5188 |    0.0305 |         - |   13560 B |
|  JwtDotNet |      small |     27.56 us |   0.0482 us |   0.0403 us | 36,290.46 |   1.70 |     0.01 |    0.7935 |    0.0305 |         - |   19912 B |

Benchmarks with issues:
  ValidateToken.JoseDotNet: Job-FTEDFH(Runtime=Core, Server=True, Toolchain=.NET Core 2.1, RunStrategy=Throughput) [token=enc-big]
  ValidateToken.JoseDotNet: Job-FTEDFH(Runtime=Core, Server=True, Toolchain=.NET Core 2.1, RunStrategy=Throughput) [token=enc-empty]
  ValidateToken.JoseDotNet: Job-FTEDFH(Runtime=Core, Server=True, Toolchain=.NET Core 2.1, RunStrategy=Throughput) [token=enc-medium]
  ValidateToken.JoseDotNet: Job-FTEDFH(Runtime=Core, Server=True, Toolchain=.NET Core 2.1, RunStrategy=Throughput) [token=enc-small]