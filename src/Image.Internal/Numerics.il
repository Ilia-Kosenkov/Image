.class public sealed abstract auto ansi Image.Internal.Numerics
{
	.method public static hidebysig !!T DangerousAdd<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		add
		ret
	}

	.method public static hidebysig !!T DangerousSubtract<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		sub
		ret
	}

	.method public static hidebysig !!T DangerousMultiply<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		mul
		ret
	}

	.method public static hidebysig !!T DangerousDivide<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		div
		ret
	}

	.method public static hidebysig !!T DangerousNegate<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T item) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		neg
		ret
	}

	.method public static hidebysig bool DangerousEquals<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldtoken		!!T
		call		class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
		ldtoken     [mscorlib]System.Double
		call        class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
        call        bool [mscorlib]System.Type::op_Equality(class [mscorlib]System.Type, class [mscorlib]System.Type)
		brfalse.s	IL_001 // If not double, goto
		ldarg.0
		ldarg.1
		call		bool [MathNet.Numerics]MathNet.Numerics.Precision::AlmostEqual(float64, float64)
		ret

		IL_001: ldtoken !!T
		call		class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
		ldtoken		[mscorlib]System.Single
		call		class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
        call        bool [mscorlib]System.Type::op_Equality(class [mscorlib]System.Type, class [mscorlib]System.Type)
		brfalse.s	IL_002 // If not double, goto
		ldarg.0
		ldarg.1
		call		bool [MathNet.Numerics]MathNet.Numerics.Precision::AlmostEqual(float32, float32)
		ret

		IL_002: 
		ldarg.0
		ldarg.1
		ceq
		ret
	}

	.method public static hidebysig bool DangerousNotEquals<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
	
		ldarg.0
		ldarg.1
		call	bool Image.Internal.Numerics::DangerousEquals<!!T>(!!T, !!T)
		ldc.i4.0
		ceq
		ret
	}

	.method public static hidebysig bool DangerousGreaterThan<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		cgt
		ret
	}

	.method public static hidebysig bool DangerousLessThan<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		clt
		ret
	}

	.method public static hidebysig bool DangerousGreaterEquals<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		cgt
		brfalse.s	IL_001
		ldc.i4.1
		ret

		IL_001:
		ldarg.0
		ldarg.1
		call		bool Image.Internal.Numerics::DangerousEquals<!!T>(!!T, !!T)
		ret
	}

	.method public static hidebysig bool DangerousLessEquals<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		clt
		brfalse.s	IL_001
		ldc.i4.1
		ret

		IL_001:
		ldarg.0
		ldarg.1
		call		bool Image.Internal.Numerics::DangerousEquals<!!T>(!!T, !!T)
		ret
	}

	.method public static hidebysig int32 Compare<valuetype .ctor (class [mscorlib]System.ValueType modreq ([mscorlib]System.Runtime.InteropServices.UnmanagedType)) T>(!!T left, !!T right) cil managed
	{
		.param type [1]
		.custom instance void System.Runtime.CompilerServices.IsUnmanagedAttribute::.ctor()
		= (01 00 00 00 )
		ldarg.0
		ldarg.1
		cgt
		brfalse.s	IL_001

		ldc.i4.1
		ret

		IL_001:
		ldarg.0
		ldarg.1
		call		bool Image.Internal.Numerics::DangerousEquals<!!T>(!!T, !!T)
		brfalse.s	IL_002

		ldc.i4.0
		ret

		IL_002:
		ldarg.0
		ldarg.1
		clt
		brfalse.s	IL_003
		
		ldc.i4.1
		neg
		ret

		IL_003:
		newobj		instance void [mscorlib]System.ArithmeticException::.ctor()
		throw
	}
}