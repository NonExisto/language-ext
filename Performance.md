# Performance benchmarks

** Machine spec **

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
AMD Ryzen 7 PRO 4750U with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
      
## Note

References to `Sys.Coll.Imm` are Microsoft's `ImmutableCollections` library.

* `Sys.Coll.Imm.List` is `System.Collections.Immutable.ImmutableList`
* `Sys.Coll.Imm.Dictionary` is `System.Collections.Immutable.ImmutableDictionary`
* `Sys.Coll.Imm.SortedDictionary` is `System.Collections.Immutable.ImmutableSortedDictionary`
* `Sys.Coll.Imm.SortedSet` is `System.Collections.Immutable.ImmutableSortedSet`
      

## Lists

### Lists with value types

#### Feed collection using Add method

| Method                         | N      | Mean            | Error         | StdDev          | Median          | Rank | Allocated   |  
|------------------------------- |------- |----------------:|--------------:|----------------:|----------------:|-----:|------------:|  
| SysColList                     | 100    |        269.9 ns |       5.45 ns |         9.84 ns |        265.5 ns |    1 |     1.16 KB |  
| SysColListWrap                 | 100    |        407.9 ns |       8.17 ns |        18.10 ns |        408.3 ns |    2 |     1.18 KB |  
| SysColImmutableList            | 100    |      8,471.5 ns |     169.00 ns |       213.73 ns |      8,441.1 ns |    8 |    33.89 KB |  
| SysColImmutableListWithBuilder | 100    |      3,573.5 ns |      18.55 ns |        17.35 ns |      3,566.9 ns |    5 |     4.71 KB |  
| LangExtLst                     | 100    |      8,669.5 ns |     157.30 ns |       139.44 ns |      8,673.7 ns |    8 |    43.39 KB |
| LangExtSeq                     | 100    |        759.0 ns |      13.45 ns |        11.93 ns |        757.5 ns |    3 |     5.77 KB |  
| SysColList                     | 1000   |      2,104.9 ns |      33.74 ns |        31.56 ns |      2,084.9 ns |    4 |     8.23 KB |  
| SysColListWrap                 | 1000   |      3,726.3 ns |      23.41 ns |        20.75 ns |      3,716.4 ns |    6 |     8.25 KB |  
| SysColImmutableList            | 1000   |    128,630.5 ns |   2,073.07 ns |     1,837.73 ns |    128,064.6 ns |   13 |   491.11 KB |  
| SysColImmutableListWithBuilder | 1000   |     47,311.8 ns |     478.07 ns |       399.21 ns |     47,068.9 ns |   11 |     46.9 KB |  
| LangExtLst                     | 1000   |    138,865.4 ns |   2,531.90 ns |     2,244.47 ns |    138,450.9 ns |   14 |   591.73 KB |  
| LangExtSeq                     | 1000   |      7,307.2 ns |      53.16 ns |        41.50 ns |      7,325.7 ns |    7 |    55.03 KB |  
| SysColList                     | 10000  |     22,456.5 ns |     443.13 ns |       544.20 ns |     22,260.8 ns |    9 |   128.32 KB |  
| SysColListWrap                 | 10000  |     40,212.5 ns |     646.67 ns |       691.93 ns |     40,261.3 ns |   10 |   128.34 KB |  
| SysColImmutableList            | 10000  |  2,704,957.5 ns |  53,432.35 ns |    65,619.73 ns |  2,714,726.8 ns |   19 |  6497.69 KB |  
| SysColImmutableListWithBuilder | 10000  |    676,652.2 ns |   6,958.60 ns |     5,810.74 ns |    673,966.7 ns |   17 |   468.77 KB |  
| LangExtLst                     | 10000  |  2,600,896.1 ns |  51,883.35 ns |    45,993.24 ns |  2,588,640.6 ns |   19 |  7512.02 KB |  
| LangExtSeq                     | 10000  |     80,515.9 ns |   1,606.60 ns |     4,451.87 ns |     80,292.6 ns |   12 |      597 KB |  
| SysColList                     | 100000 |    352,060.3 ns |   6,791.21 ns |     6,352.50 ns |    350,350.7 ns |   15 |  1024.48 KB |  
| SysColListWrap                 | 100000 |    548,609.1 ns |  10,936.96 ns |    23,307.55 ns |    541,110.1 ns |   16 |  1024.51 KB |  
| SysColImmutableList            | 100000 | 42,737,972.3 ns | 825,671.67 ns | 1,260,887.95 ns | 42,736,358.3 ns |   22 | 80575.05 KB |  
| SysColImmutableListWithBuilder | 100000 | 10,399,443.4 ns | 205,434.96 ns |   442,220.46 ns | 10,390,200.0 ns |   20 |  4687.53 KB |  
| LangExtLst                     | 100000 | 39,276,407.5 ns | 783,646.00 ns | 1,831,747.41 ns | 39,368,885.7 ns |   21 | 90729.75 KB |  
| LangExtSeq                     | 100000 |    935,333.7 ns |  18,651.52 ns |    40,149.36 ns |    925,822.0 ns |   18 |  5711.91 KB |
#### Iterate Collection using foreach loop
| Method              | N      | Mean            | Error         | StdDev        | Rank | Allocated |
|-------------------- |------- |----------------:|--------------:|--------------:|-----:|----------:|
| SysArray            | 100    |        31.00 ns |      0.026 ns |      0.024 ns |    1 |         - |
| SysColImmutableList | 100    |       574.64 ns |      8.376 ns |      6.994 ns |    5 |         - |
| LangExtLst          | 100    |       444.52 ns |      8.811 ns |     16.111 ns |    4 |      32 B |
| LangExtSeq          | 100    |       206.07 ns |      4.099 ns |      5.878 ns |    2 |      40 B |
| SysArray            | 1000   |       247.47 ns |      4.101 ns |      3.635 ns |    3 |         - |
| SysColImmutableList | 1000   |     6,075.30 ns |    120.995 ns |    233.117 ns |    9 |         - |
| LangExtLst          | 1000   |     4,461.60 ns |     88.634 ns |    251.439 ns |    8 |      32 B |
| LangExtSeq          | 1000   |     1,860.62 ns |      0.951 ns |      0.889 ns |    6 |      40 B |
| SysArray            | 10000  |     2,520.11 ns |     13.685 ns |     12.801 ns |    7 |         - |
| SysColImmutableList | 10000  |    62,516.21 ns |    451.542 ns |    422.373 ns |   13 |         - |
| LangExtLst          | 10000  |    41,962.61 ns |    990.653 ns |  2,920.962 ns |   12 |      32 B |
| LangExtSeq          | 10000  |    18,436.33 ns |      4.824 ns |      4.028 ns |   10 |      40 B |
| SysArray            | 100000 |    23,965.13 ns |     24.681 ns |     21.879 ns |   11 |         - |
| SysColImmutableList | 100000 | 1,240,831.50 ns | 25,974.189 ns | 73,260.665 ns |   16 |       1 B |
| LangExtLst          | 100000 |   882,262.44 ns | 17,428.814 ns | 45,608.172 ns |   15 |      32 B |
| LangExtSeq          | 100000 |   184,019.80 ns |    103.957 ns |     81.163 ns |   14 |      40 B |

## Maps

### Maps with value types

#### Feed collection using Add method

| Method       (Add)                         | N      | Mean           | Error         | StdDev        | Median         | Rank | Allocated    |
|------------------------------------------- |------- |---------------:|--------------:|--------------:|---------------:|-----:|-------------:|
| SysColImmutableDictionary                  | 100    |      13.808 us |     0.0238 us |     0.0211 us |      13.811 us |    8 |     38.58 KB |
| SysColImmutableDictionaryWithBuilder       | 100    |       6.498 us |     0.0062 us |     0.0052 us |       6.499 us |    3 |      5.51 KB |
| SysColImmutableSortedDictionary            | 100    |      10.248 us |     0.0234 us |     0.0219 us |      10.250 us |    6 |     34.41 KB |
| SysColImmutableSortedDictionaryWithBuilder | 100    |       5.503 us |     0.0058 us |     0.0052 us |       5.502 us |    2 |      4.73 KB |
| SasaTrie                                   | 100    |       6.751 us |     0.0186 us |     0.0165 us |       6.753 us |    4 |     47.48 KB |
| SysColDictionary                           | 100    |       1.291 us |     0.0017 us |     0.0015 us |       1.291 us |    1 |      7.22 KB |
| LangExtHashMap                             | 100    |       6.462 us |     0.1264 us |     0.1505 us |       6.392 us |    3 |     32.45 KB |
| LangExtMap                                 | 100    |       9.027 us |     0.1307 us |     0.1159 us |       8.993 us |    5 |     40.98 KB |
| SysColImmutableDictionary                  | 1000   |     302.576 us |     2.1295 us |     1.9920 us |     301.934 us |   15 |       568 KB |
| SysColImmutableDictionaryWithBuilder       | 1000   |     151.916 us |     0.9358 us |     0.8754 us |     151.436 us |   12 |     54.73 KB |
| SysColImmutableSortedDictionary            | 1000   |     226.626 us |     4.3430 us |     5.1701 us |     224.807 us |   14 |    500.25 KB |
| SysColImmutableSortedDictionaryWithBuilder | 1000   |     106.475 us |     0.4449 us |     0.3715 us |     106.606 us |    9 |     46.92 KB |
| SasaTrie                                   | 1000   |     132.887 us |     2.0604 us |     1.8265 us |     132.978 us |   11 |    917.95 KB |
| SysColDictionary                           | 1000   |      12.244 us |     0.2386 us |     0.2343 us |      12.185 us |    7 |     71.45 KB |
| LangExtHashMap                             | 1000   |     118.215 us |     2.2898 us |     2.4500 us |     116.804 us |   10 |     510.9 KB |
| LangExtMap                                 | 1000   |     174.465 us |     3.4663 us |     4.7447 us |     174.818 us |   13 |    565.74 KB |
| SysColImmutableDictionary                  | 10000  |   5,819.000 us |   124.6758 us |   349.6044 us |   5,667.700 us |   22 |    7575.2 KB |
| SysColImmutableDictionaryWithBuilder       | 10000  |   2,287.623 us |     5.5825 us |     4.6616 us |   2,287.641 us |   17 |    546.92 KB |
| SysColImmutableSortedDictionary            | 10000  |   4,262.348 us |    80.9914 us |   143.9621 us |   4,199.267 us |   21 |   6626.96 KB |
| SysColImmutableSortedDictionaryWithBuilder | 10000  |   1,620.884 us |     8.2774 us |     7.3377 us |   1,619.336 us |   16 |     468.8 KB |
| SasaTrie                                   | 10000  |   2,899.309 us |     9.4055 us |     7.3432 us |   2,898.791 us |   19 |  14982.57 KB |
| SysColDictionary                           | 10000  |     300.249 us |     3.6207 us |     3.3868 us |     301.739 us |   15 |    657.33 KB |
| LangExtHashMap                             | 10000  |   2,366.062 us |    13.0573 us |    11.5749 us |   2,364.818 us |   18 |   7291.81 KB |
| LangExtMap                                 | 10000  |   3,618.933 us |    52.6270 us |    43.9460 us |   3,607.049 us |   20 |   7320.76 KB |
| SysColImmutableDictionary                  | 100000 | 149,745.801 us | 2,975.7710 us | 4,544.3170 us | 149,632.675 us |   29 |   94286.8 KB |
| SysColImmutableDictionaryWithBuilder       | 100000 |  40,833.756 us |   252.0743 us |   210.4934 us |  40,813.342 us |   24 |   5468.82 KB |
| SysColImmutableSortedDictionary            | 100000 | 103,044.229 us |   876.4747 us |   731.8961 us | 102,986.300 us |   27 |  82156.79 KB |
| SysColImmutableSortedDictionaryWithBuilder | 100000 |  28,753.605 us |   236.3851 us |   184.5539 us |  28,746.675 us |   23 |   4687.57 KB |
| SasaTrie                                   | 100000 |  88,001.983 us | 1,690.5252 us | 1,581.3183 us |  88,221.667 us |   26 | 194994.94 KB |
| SysColDictionary                           | 100000 |   3,676.909 us |    37.0527 us |    30.9407 us |   3,670.912 us |   20 |   5896.27 KB |
| LangExtHashMap                             | 100000 |  57,600.342 us | 1,142.4647 us | 1,563.8186 us |  57,246.950 us |   25 |  92364.78 KB |
| LangExtMap                                 | 100000 | 115,507.115 us | 2,286.3522 us | 3,491.5016 us | 115,674.420 us |   28 |  89033.28 KB |

#### Search Collection using Contains method
| Method        (Contains)        | N      | Mean            | Error         | StdDev          | Median          | Rank | Allocated |        
|-------------------------------- |------- |----------------:|--------------:|----------------:|----------------:|-----:|----------:|        
| SysColImmutableDictionary       | 100    |        673.1 ns |       6.85 ns |         6.41 ns |        672.2 ns |    3 |         - |        
| SasaTrie                        | 100    |        596.3 ns |       0.29 ns |         0.26 ns |        596.3 ns |    2 |         - |        
| SysColImmutableSortedDictionary | 100    |      1,417.7 ns |      27.57 ns |        31.74 ns |      1,420.3 ns |    5 |         - |        
| SysColDictionary                | 100    |        423.2 ns |       0.36 ns |         0.30 ns |        423.2 ns |    1 |         - |        
| LangExtHashMap                  | 100    |      1,121.3 ns |       1.80 ns |         1.68 ns |      1,121.7 ns |    4 |         - |        
| LangExtMap                      | 100    |      2,650.9 ns |       0.97 ns |         0.86 ns |      2,650.8 ns |    6 |         - |        
| SysColImmutableDictionary       | 1000   |     25,262.1 ns |     502.94 ns |     1,359.73 ns |     25,636.0 ns |   10 |         - |        
| SasaTrie                        | 1000   |      9,505.9 ns |      10.69 ns |         9.48 ns |      9,507.6 ns |    8 |         - |        
| SysColImmutableSortedDictionary | 1000   |     32,422.7 ns |     337.83 ns |       282.10 ns |     32,378.9 ns |   11 |         - |        
| SysColDictionary                | 1000   |      4,250.2 ns |       6.35 ns |         5.30 ns |      4,249.2 ns |    7 |         - |        
| LangExtHashMap                  | 1000   |     14,840.6 ns |      20.21 ns |        18.90 ns |     14,839.6 ns |    9 |         - |        
| LangExtMap                      | 1000   |     43,436.0 ns |      86.56 ns |        80.97 ns |     43,453.4 ns |   12 |         - |        
| SysColImmutableDictionary       | 10000  |    617,069.2 ns |   1,640.87 ns |     1,454.58 ns |    617,617.7 ns |   16 |         - |        
| SasaTrie                        | 10000  |    147,483.4 ns |     195.77 ns |       183.13 ns |    147,522.6 ns |   14 |         - |        
| SysColImmutableSortedDictionary | 10000  |    802,847.9 ns |     594.72 ns |       556.30 ns |    802,902.2 ns |   17 |         - |        
| SysColDictionary                | 10000  |     64,264.9 ns |     495.85 ns |       439.56 ns |     64,342.0 ns |   13 |         - |        
| LangExtHashMap                  | 10000  |    208,653.7 ns |      59.76 ns |        49.90 ns |    208,645.9 ns |   15 |         - |        
| LangExtMap                      | 10000  |    837,175.4 ns |     944.46 ns |       883.44 ns |    837,513.0 ns |   18 |         - |        
| SysColImmutableDictionary       | 100000 | 13,332,795.2 ns | 255,998.07 ns |   350,413.08 ns | 13,112,189.8 ns |   22 |       6 B |        
| SasaTrie                        | 100000 |  2,429,243.9 ns |  32,621.28 ns |    28,917.91 ns |  2,423,538.7 ns |   20 |       2 B |        
| SysColImmutableSortedDictionary | 100000 | 15,254,388.8 ns | 261,891.39 ns |   244,973.37 ns | 15,150,020.3 ns |   23 |       6 B |
| SysColDictionary                | 100000 |  1,194,459.4 ns |  17,376.68 ns |    16,254.16 ns |  1,198,207.5 ns |   19 |       1 B |        
| LangExtHashMap                  | 100000 |  4,053,155.9 ns | 116,567.46 ns |   328,780.60 ns |  4,010,564.3 ns |   21 |       2 B |        
| LangExtMap                      | 100000 | 16,997,394.2 ns | 369,746.64 ns | 1,048,909.74 ns | 16,505,775.0 ns |   24 |      12 B |

#### Iterate Collection using foreach loop
| Method         (Iterate)        | N      | Mean            | Error          | StdDev         | Median          | Rank | Allocated |        
|-------------------------------- |------- |----------------:|---------------:|---------------:|----------------:|-----:|----------:|        
| SysColImmutableDictionary       | 100    |     2,881.76 ns |      56.614 ns |      65.197 ns |     2,860.51 ns |    7 |         - |        
| SysColImmutableSortedDictionary | 100    |       503.02 ns |       9.353 ns |      10.771 ns |       499.60 ns |    3 |         - |        
| SasaTrie                        | 100    |     1,911.46 ns |      20.515 ns |      21.951 ns |     1,905.86 ns |    6 |    2184 B |
| SysColDictionary                | 100    |        81.33 ns |       0.076 ns |       0.067 ns |        81.36 ns |    1 |         - |        
| LangExtHashMap                  | 100    |     1,802.76 ns |       3.041 ns |       2.374 ns |     1,803.29 ns |    5 |    2240 B |        
| LangExtMap                      | 100    |       401.02 ns |       7.875 ns |      12.716 ns |       400.01 ns |    2 |      32 B |        
| SysColImmutableDictionary       | 1000   |    23,391.51 ns |     280.279 ns |     248.460 ns |    23,386.75 ns |   13 |         - |        
| SysColImmutableSortedDictionary | 1000   |     5,470.44 ns |      44.853 ns |      39.761 ns |     5,467.06 ns |    9 |         - |        
| SasaTrie                        | 1000   |    19,215.60 ns |      85.646 ns |      75.923 ns |    19,227.66 ns |   11 |   16664 B |        
| SysColDictionary                | 1000   |       733.62 ns |       2.096 ns |       1.858 ns |       732.98 ns |    4 |         - |        
| LangExtHashMap                  | 1000   |    21,228.41 ns |     236.220 ns |     209.403 ns |    21,114.02 ns |   12 |   19968 B |        
| LangExtMap                      | 1000   |     4,679.39 ns |      93.128 ns |     198.463 ns |     4,722.73 ns |    8 |      32 B |        
| SysColImmutableDictionary       | 10000  |   253,351.81 ns |     552.379 ns |     489.669 ns |   253,365.55 ns |   18 |         - |        
| SysColImmutableSortedDictionary | 10000  |    85,416.15 ns |     807.029 ns |     754.896 ns |    85,657.09 ns |   15 |         - |        
| SasaTrie                        | 10000  |   226,076.61 ns |     453.666 ns |     424.359 ns |   226,205.96 ns |   16 |  189504 B |        
| SysColDictionary                | 10000  |     7,346.62 ns |      12.590 ns |      11.777 ns |     7,345.20 ns |   10 |         - |        
| LangExtHashMap                  | 10000  |   236,224.61 ns |     239.671 ns |     224.188 ns |   236,188.33 ns |   17 |  149248 B |        
| LangExtMap                      | 10000  |    71,664.96 ns |   1,420.697 ns |   1,259.411 ns |    71,911.05 ns |   14 |      32 B |        
| SysColImmutableDictionary       | 100000 | 6,146,062.23 ns | 122,146.350 ns | 268,114.219 ns | 6,067,064.45 ns |   23 |       3 B |        
| SysColImmutableSortedDictionary | 100000 | 2,523,253.44 ns |  49,710.970 ns | 114,219.228 ns | 2,459,138.28 ns |   20 |       2 B |        
| SasaTrie                        | 100000 | 3,082,409.04 ns |  78,762.150 ns | 224,712.862 ns | 2,993,396.09 ns |   21 | 1816739 B |        
| SysColDictionary                | 100000 |    73,587.49 ns |     589.000 ns |     459.852 ns |    73,496.62 ns |   14 |         - |        
| LangExtHashMap                  | 100000 | 4,135,406.67 ns |  12,911.565 ns |  10,781.742 ns | 4,136,525.00 ns |   22 | 2063075 B |        
| LangExtMap                      | 100000 | 1,845,664.03 ns |  36,477.600 ns |  44,797.772 ns | 1,845,749.61 ns |   19 |      33 B |  

#### Clear collection using Remove method
| Method          (Remove)        | N      | Mean             | Error           | StdDev          | Median           | Rank |
|-------------------------------- |------- |-----------------:|----------------:|----------------:|-----------------:|-----:|
| SysColImmutableDictionary       | 100    |      10,115.6 ns |       178.90 ns |       226.25 ns |      10,076.4 ns |    7 |
| SysColImmutableSortedDictionary | 100    |       7,753.1 ns |        38.11 ns |        33.78 ns |       7,739.6 ns |    5 |
| SasaTrie                        | 100    |       8,095.5 ns |        21.18 ns |        19.81 ns |       8,098.1 ns |    6 |
| SysColDictionary                | 100    |         162.3 ns |         0.22 ns |         0.20 ns |         162.3 ns |    1 |
| LangExtHashMap                  | 100    |       6,254.8 ns |        13.13 ns |        11.64 ns |       6,251.8 ns |    3 |
| LangExtMap                      | 100    |       6,782.3 ns |         8.07 ns |         7.16 ns |       6,784.1 ns |    4 |
| SysColImmutableDictionary       | 1000   |     230,039.0 ns |       542.02 ns |       480.48 ns |     229,900.3 ns |   13 |
| SysColImmutableSortedDictionary | 1000   |     173,172.2 ns |       263.29 ns |       246.29 ns |     173,139.4 ns |   12 |
| SasaTrie                        | 1000   |     138,877.3 ns |       215.59 ns |       201.67 ns |     138,874.3 ns |   10 |
| SysColDictionary                | 1000   |       1,580.9 ns |         1.58 ns |         1.40 ns |       1,581.2 ns |    2 |
| LangExtHashMap                  | 1000   |     111,037.5 ns |       207.29 ns |       173.09 ns |     111,046.7 ns |    9 |
| LangExtMap                      | 1000   |     143,434.1 ns |       236.88 ns |       209.99 ns |     143,357.3 ns |   11 |
| SysColImmutableDictionary       | 10000  |   4,501,599.7 ns |    25,162.84 ns |    23,537.33 ns |   4,505,164.1 ns |   19 |
| SysColImmutableSortedDictionary | 10000  |   3,453,558.9 ns |    10,221.34 ns |     9,060.95 ns |   3,455,174.8 ns |   18 |
| SasaTrie                        | 10000  |   3,103,572.8 ns |    56,567.31 ns |    44,164.04 ns |   3,083,403.9 ns |   17 |
| SysColDictionary                | 10000  |      16,843.0 ns |       154.08 ns |       144.13 ns |      16,785.7 ns |    8 |
| LangExtHashMap                  | 10000  |   2,269,033.9 ns |    44,381.49 ns |    86,562.69 ns |   2,256,985.0 ns |   15 |
| LangExtMap                      | 10000  |   2,944,202.2 ns |    38,460.09 ns |    32,115.92 ns |   2,926,842.2 ns |   16 |
| SysColImmutableDictionary       | 100000 | 107,720,398.5 ns | 2,144,850.28 ns | 5,055,664.82 ns | 105,068,520.0 ns |   23 |
| SysColImmutableSortedDictionary | 100000 |  86,091,627.9 ns | 1,712,562.95 ns | 2,908,062.09 ns |  84,647,733.3 ns |   22 |
| SasaTrie                        | 100000 |  90,682,359.5 ns | 1,427,587.19 ns | 1,265,518.86 ns |  91,118,358.3 ns |   22 |
| SysColDictionary                | 100000 |     239,513.8 ns |     1,841.50 ns |     1,537.74 ns |     238,809.4 ns |   14 |
| LangExtHashMap                  | 100000 |  51,775,093.4 ns |   912,779.84 ns | 1,843,860.44 ns |  51,025,570.0 ns |   20 |
| LangExtMap                      | 100000 |  80,360,125.6 ns |   771,214.12 ns |   643,998.72 ns |  80,403,366.7 ns |   21 |


## Sets

### Unsorted sets with value types

#### Feed collection using Add method
| Method               (Add)          | N      | Mean           | Error         | StdDev        | Median         | Rank | Allocated   |
|------------------------------------ |------- |---------------:|--------------:|--------------:|---------------:|-----:|------------:|
| SysColImmutableHashSet              | 100    |      12.893 us |     0.1986 us |     0.1658 us |      12.861 us |    8 |    39.36 KB |
| SysColImmutableHashSetWithBuilder   | 100    |       6.586 us |     0.0540 us |     0.0451 us |       6.577 us |    4 |     5.52 KB |
| SysColImmutableSortedSet            | 100    |       9.556 us |     0.1880 us |     0.3193 us |       9.457 us |    6 |    32.84 KB |
| SysColImmutableSortedSetWithBuilder | 100    |       5.268 us |     0.0429 us |     0.0358 us |       5.255 us |    2 |     4.72 KB |
| SysColHashSet                       | 100    |       1.219 us |     0.0227 us |     0.0212 us |       1.212 us |    1 |     5.86 KB |
| LangExtHashSet                      | 100    |       6.073 us |     0.1114 us |     0.0988 us |       6.025 us |    3 |     28.1 KB |
| LangExtSet                          | 100    |       8.561 us |     0.0138 us |     0.0107 us |       8.558 us |    5 |     40.6 KB |
| SysColImmutableHashSet              | 1000   |     284.929 us |     4.2338 us |     6.3370 us |     281.552 us |   15 |   578.49 KB |
| SysColImmutableHashSetWithBuilder   | 1000   |     142.819 us |     1.2497 us |     1.1079 us |     142.554 us |   11 |    54.73 KB |
| SysColImmutableSortedSet            | 1000   |     210.419 us |     2.1130 us |     1.9765 us |     209.246 us |   13 |   486.92 KB |
| SysColImmutableSortedSetWithBuilder | 1000   |     106.622 us |     0.4336 us |     0.3621 us |     106.557 us |    9 |    46.91 KB |
| SysColHashSet                       | 1000   |      11.007 us |     0.1752 us |     0.1875 us |      10.938 us |    7 |    57.29 KB |
| LangExtHashSet                      | 1000   |     111.012 us |     1.6457 us |     1.5394 us |     110.242 us |   10 |      477 KB |
| LangExtSet                          | 1000   |     170.134 us |     1.3688 us |     1.1430 us |     169.887 us |   12 |   572.21 KB |
| SysColImmutableHashSet              | 10000  |   5,176.150 us |    24.4301 us |    22.8519 us |   5,177.777 us |   22 |  7626.91 KB |
| SysColImmutableHashSetWithBuilder   | 10000  |   2,211.867 us |     5.9037 us |     5.2334 us |   2,210.207 us |   17 |   546.92 KB |
| SysColImmutableSortedSet            | 10000  |   4,025.570 us |    16.4677 us |    15.4039 us |   4,024.133 us |   21 |  6448.07 KB |
| SysColImmutableSortedSetWithBuilder | 10000  |   1,565.279 us |     5.2215 us |     4.0766 us |   1,564.864 us |   16 |   468.78 KB |
| SysColHashSet                       | 10000  |     252.976 us |     4.9728 us |     4.4083 us |     252.134 us |   14 |   526.03 KB |
| LangExtHashSet                      | 10000  |   2,385.229 us |    36.7997 us |    30.7294 us |   2,377.804 us |   18 |  7062.04 KB |
| LangExtSet                          | 10000  |   3,613.593 us |    21.7033 us |    18.1232 us |   3,613.568 us |   20 |  7282.79 KB |
| SysColImmutableHashSet              | 100000 | 140,304.090 us | 2,678.6353 us | 2,866.1098 us | 140,618.788 us |   28 | 94689.62 KB |
| SysColImmutableHashSetWithBuilder   | 100000 |  38,559.544 us |   400.7814 us |   374.8912 us |  38,639.521 us |   24 |  5468.83 KB |
| SysColImmutableSortedSet            | 100000 |  99,236.769 us |   543.0968 us |   453.5104 us |  99,211.483 us |   26 | 80269.87 KB |
| SysColImmutableSortedSetWithBuilder | 100000 |  27,350.774 us |   163.7672 us |   145.1753 us |  27,354.519 us |   23 |  4687.55 KB |
| SysColHashSet                       | 100000 |   3,143.114 us |    62.6662 us |   175.7228 us |   3,229.150 us |   19 |  4717.39 KB |
| LangExtHashSet                      | 100000 |  53,085.868 us |   935.9416 us |   875.4803 us |  53,068.890 us |   25 | 89629.82 KB |
| LangExtSet                          | 100000 | 112,875.564 us | 2,186.8242 us | 2,147.7537 us | 113,129.230 us |   27 | 88669.88 KB |

#### Search Collection using Contains method
| Method     (Contains)    | N      | Mean            | Error        | StdDev       | Rank |
|------------------------- |------- |----------------:|-------------:|-------------:|-----:|
| SysColImmutableHashSet   | 100    |        560.4 ns |      4.61 ns |      3.85 ns |    2 |
| SysColImmutableSortedSet | 100    |      1,389.1 ns |      3.29 ns |      2.91 ns |    4 |
| SysColHashSet            | 100    |        439.6 ns |      1.89 ns |      1.67 ns |    1 |
| LangExtHashSet           | 100    |      1,404.1 ns |      0.60 ns |      0.56 ns |    4 |
| LangExtSet               | 100    |      1,232.1 ns |      3.12 ns |      2.92 ns |    3 |
| SysColImmutableHashSet   | 1000   |     16,034.3 ns |  1,052.69 ns |  3,103.87 ns |    6 |
| SysColImmutableSortedSet | 1000   |     28,946.5 ns |    405.70 ns |    379.49 ns |    8 |
| SysColHashSet            | 1000   |      4,659.9 ns |      8.90 ns |      7.89 ns |    5 |
| LangExtHashSet           | 1000   |     17,615.8 ns |     57.46 ns |     44.86 ns |    6 |
| LangExtSet               | 1000   |     27,381.7 ns |    531.74 ns |    497.39 ns |    7 |
| SysColImmutableHashSet   | 10000  |    582,121.9 ns |  2,235.57 ns |  1,981.77 ns |   11 |
| SysColImmutableSortedSet | 10000  |    786,235.5 ns |    982.03 ns |    918.59 ns |   12 |
| SysColHashSet            | 10000  |     67,868.0 ns |    294.48 ns |    261.05 ns |    9 |
| LangExtHashSet           | 10000  |    237,942.3 ns |    165.28 ns |    154.60 ns |   10 |
| LangExtSet               | 10000  |    778,237.8 ns |  3,064.31 ns |  2,866.36 ns |   12 |
| SysColImmutableHashSet   | 100000 | 12,411,250.9 ns | 14,215.28 ns | 12,601.47 ns |   15 |
| SysColImmutableSortedSet | 100000 | 14,375,356.0 ns | 19,755.26 ns | 17,512.52 ns |   16 |
| SysColHashSet            | 100000 |  1,138,017.4 ns |  1,682.91 ns |  1,491.86 ns |   13 |
| LangExtHashSet           | 100000 |  3,643,003.0 ns | 21,061.89 ns | 19,701.31 ns |   14 |
| LangExtSet               | 100000 | 14,339,463.0 ns | 22,800.24 ns | 21,327.36 ns |   16 |

#### Iterate Collection using foreach loop
| Method        (iter)     | N      | Mean            | Error         | StdDev        | Rank | Allocated |
|------------------------- |------- |----------------:|--------------:|--------------:|-----:|----------:|
| SysColImmutableHashSet   | 100    |     3,011.89 ns |     15.153 ns |     13.433 ns |    6 |         - |
| SysColImmutableSortedSet | 100    |       625.41 ns |      1.635 ns |      1.450 ns |    3 |         - |
| SysColHashSet            | 100    |        91.52 ns |      0.024 ns |      0.022 ns |    1 |         - |
| LangExtHashSet           | 100    |     1,464.21 ns |     11.810 ns |     11.047 ns |    5 |    2176 B |
| LangExtSet               | 100    |       531.55 ns |      1.729 ns |      1.350 ns |    2 |      88 B |
| SysColImmutableHashSet   | 1000   |    33,153.21 ns |     29.971 ns |     25.027 ns |   10 |         - |
| SysColImmutableSortedSet | 1000   |     6,734.15 ns |     59.471 ns |     55.629 ns |    7 |         - |
| SysColHashSet            | 1000   |       856.58 ns |      0.119 ns |      0.100 ns |    4 |         - |
| LangExtHashSet           | 1000   |    15,418.23 ns |     11.563 ns |     10.250 ns |    9 |   19584 B |
| LangExtSet               | 1000   |     6,612.61 ns |     28.575 ns |     26.729 ns |    7 |      88 B |
| SysColImmutableHashSet   | 10000  |   312,138.89 ns |  2,139.898 ns |  2,001.662 ns |   14 |         - |
| SysColImmutableSortedSet | 10000  |    94,793.48 ns |    200.410 ns |    177.658 ns |   12 |         - |
| SysColHashSet            | 10000  |     8,499.98 ns |      7.186 ns |      6.370 ns |    8 |         - |
| LangExtHashSet           | 10000  |   186,259.32 ns |    391.094 ns |    365.829 ns |   13 |  152864 B |
| LangExtSet               | 10000  |    84,055.27 ns |  1,625.864 ns |  1,669.643 ns |   11 |      88 B |
| SysColImmutableHashSet   | 100000 | 5,609,745.07 ns | 40,555.816 ns | 33,865.943 ns |   18 |       3 B |
| SysColImmutableSortedSet | 100000 | 2,618,829.63 ns | 51,904.733 ns | 83,816.476 ns |   16 |       2 B |
| SysColHashSet            | 100000 |    85,226.56 ns |     23.124 ns |     18.054 ns |   11 |         - |
| LangExtHashSet           | 100000 | 2,950,603.41 ns |  8,338.652 ns |  7,799.980 ns |   17 | 2056034 B |
| LangExtSet               | 100000 | 2,124,616.43 ns | 21,953.993 ns | 20,535.779 ns |   15 |      90 B |

#### Clear collection using Remove method
| Method     (Removal)     | N      | Mean             | Error           | StdDev          | Rank | Allocated  |
|------------------------- |------- |-----------------:|----------------:|----------------:|-----:|-----------:|
| SysColImmutableHashSet   | 100    |       8,800.0 ns |        46.13 ns |        43.15 ns |    5 |    31008 B |
| SysColImmutableSortedSet | 100    |       7,183.3 ns |        31.78 ns |        28.17 ns |    4 |    25632 B |
| SysColHashSet            | 100    |         169.9 ns |         0.15 ns |         0.13 ns |    1 |          - |
| LangExtHashSet           | 100    |       6,395.7 ns |        12.57 ns |        11.14 ns |    3 |    29352 B |
| LangExtSet               | 100    |       8,821.9 ns |        15.50 ns |        12.94 ns |    5 |    43072 B |
| SysColImmutableHashSet   | 1000   |     220,419.6 ns |     1,308.63 ns |     1,224.09 ns |    9 |   495496 B |
| SysColImmutableSortedSet | 1000   |     171,341.0 ns |     1,264.77 ns |     1,056.14 ns |    8 |   415536 B |
| SysColHashSet            | 1000   |       1,648.7 ns |         1.36 ns |         1.20 ns |    2 |          - |
| LangExtHashSet           | 1000   |     108,618.4 ns |       258.37 ns |       215.75 ns |    7 |   487344 B |
| LangExtSet               | 1000   |     168,214.3 ns |       473.36 ns |       395.28 ns |    8 |   609184 B |
| SysColImmutableHashSet   | 10000  |   4,494,899.2 ns |    89,120.34 ns |   201,159.47 ns |   14 |  6835331 B |
| SysColImmutableSortedSet | 10000  |   3,554,833.4 ns |    70,647.86 ns |    75,592.42 ns |   13 |  5767394 B |
| SysColHashSet            | 10000  |      17,301.9 ns |        83.92 ns |        78.50 ns |    6 |          - |
| LangExtHashSet           | 10000  |   2,209,781.6 ns |    21,369.31 ns |    17,844.34 ns |   11 |  7178410 B |
| LangExtSet               | 10000  |   3,399,949.5 ns |     9,304.64 ns |     7,769.80 ns |   12 |  7778992 B |
| SysColImmutableHashSet   | 100000 | 102,043,561.7 ns |   521,100.33 ns |   406,840.86 ns |   17 | 87239022 B |
| SysColImmutableSortedSet | 100000 |  85,142,023.8 ns | 1,550,416.91 ns | 1,374,404.21 ns |   16 | 73862035 B |
| SysColHashSet            | 100000 |     241,819.7 ns |       285.72 ns |       253.28 ns |   10 |          - |
| LangExtHashSet           | 100000 |  49,172,230.7 ns |   681,393.65 ns |   637,376.04 ns |   15 | 91357174 B |
| LangExtSet               | 100000 |  98,579,630.0 ns | 1,332,315.50 ns | 1,040,184.31 ns |   17 | 94083552 B |


<style>
table tbody tr:nth-child(even){
    background-color:silver;
}
</style>