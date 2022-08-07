﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using km;

// Init function
var global = new dotnet.Benchmark(); // Init global benchmark object
dotnet._Global.global = global; // Suppress GC
dotnet.Benchmark.Ready(global); // Give the C code the pointer to the Benchmark class.

namespace dotnet
{

    public static class _Global
    {
        public static dotnet.Benchmark? global = null;
    }

    public unsafe class Benchmark
    {
        public IntPtr InputMemory = Marshal.AllocHGlobal(8_000_000);
        public IntPtr OutputMemory = Marshal.AllocHGlobal(8_000_000);

        public km.Monsters Structure = new km.Monsters();
        public Karmem.Reader Reader;
        public Karmem.Writer Writer;

        public Benchmark()
        {
            Reader = Karmem.Reader.NewReader(InputMemory, 8_000_000, 8_000_000);
            Writer = Karmem.Writer.NewFixedWriter(OutputMemory, 8_000_000);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Ready(Benchmark owner);

        public uint InputMemoryPointer()
        {
            return (uint)InputMemory.ToInt64();
        }

        public uint OutputMemoryPointer()
        {
            return (uint)OutputMemory.ToInt64();
        }

        // Must be exported to WASM.
        public void KBenchmarkEncodeObjectAPI()
        {
            this.Writer.Reset();
            if (!Structure.WriteAsRoot(this.Writer))
            {
                throw new System.Exception("Failed to write object");
            }
        }

        // Must be exported to WASM.
        public void KBenchmarkDecodeObjectAPI(uint size)
        {
            this.Reader.SetSize(size);
            Structure.ReadAsRoot(this.Reader);
        }

        // Must be exported to WASM.
        public void KBenchmarkDecodeObjectAPIFrom(byte[] b)
        {
            var reader = Karmem.Reader.NewManagedReader(b);
            Structure.ReadAsRoot(reader);
            reader.Dispose();
        }

        // Must be exported to WASM.
        public float KBenchmarkDecodeSumVec3(uint size)
        {
            this.Reader.SetSize(size);

            var monsters = MonstersViewer.NewMonstersViewer(this.Reader, 0);
            var monstersList = monsters.Monsters(this.Reader);

            var sum = new Vec3();
            for (var i = (ulong)0; i < monstersList.Count; i++)
            {
                var paths = monstersList[i].Data(this.Reader).Path(this.Reader);
                for (var p = (ulong)0; p < paths.Count; p++)
                {
                    var path = paths[p];
                    sum._X += path.X();
                    sum._Y += path.Y();
                    sum._Z += path.Z();
                }
            }
            
            return sum._X + sum._Y + sum._Z;
        }

    }
}