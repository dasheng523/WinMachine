using System;
using System.IO.Ports;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Devices.Sensors.Core;

// SerialOp<A> is essentially a Reader Monad: SerialPort -> Fin<A>
// But we wrap it to provide extension methods for LINQ.
public delegate Fin<A> SerialOp<A>(ITextLinePort port);

public static class SerialOp
{
    // Return unit
    public static SerialOp<A> Return<A>(A value) => _ => FinSucc(value);

    // Fail
    public static SerialOp<A> Fail<A>(Error error) => _ => FinFail<A>(error);

    // Primitive: Write
    public static SerialOp<Unit> Write(string text) => port =>
    {
        try
        {
            port.Write(text);
            return FinSucc(unit);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New(ex));
        }
    };
    
    // Primitive: ReadLine
    public static SerialOp<string> ReadLine() => port =>
    {
        try
        {
            return FinSucc(port.ReadLine());
        }
        catch (Exception ex)
        {
            return FinFail<string>(Error.New(ex));
        }
    };

    // Primitive: Clear Buffer
    public static SerialOp<Unit> DiscardInBuffer() => port =>
    {
        try
        {
            port.DiscardInBuffer();
            return FinSucc(unit);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New(ex));
        }
    };
    
    // Bind
    public static SerialOp<B> Bind<A, B>(this SerialOp<A> ma, Func<A, SerialOp<B>> f) => port =>
        ma(port).Bind(a => f(a)(port));

    // Map
    public static SerialOp<B> Map<A, B>(this SerialOp<A> ma, Func<A, B> f) => port =>
        ma(port).Map(f);
        
    // LINQ Select
    public static SerialOp<B> Select<A, B>(this SerialOp<A> ma, Func<A, B> f) => Map(ma, f);

    // LINQ SelectMany
    public static SerialOp<C> SelectMany<A, B, C>(
        this SerialOp<A> ma,
        Func<A, SerialOp<B>> bind,
        Func<A, B, C> project) => port =>
            ma(port).Bind(a => 
                bind(a)(port).Map(b => project(a, b)));
}


