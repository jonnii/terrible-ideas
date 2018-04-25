using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ConsoleApp2
{
    public class Output
    {
        private int MarketValue { get; set; }
    }

    public class EngineBuilder<TInput, TOutput>
    {

    }

    public static class EngineBuilderExtensions
    {
        public static void Add<TInput, TOutput, TCalculatorInput>(this EngineBuilder<TInput, TOutput> engine,
            ICalculator<TCalculatorInput> calculator)
            where TInput : TCalculatorInput
        {

        }
    }

    public class Engine
    {
        public static Engine<TInput, TOutput> For<TInput, TOutput>(Action<EngineBuilder<TInput, TOutput>> builder)
        {
            return new Engine<TInput, TOutput>();
        }
    }

    public class Engine<TInput, TOutput>
    {
        public MutableFrame<TInput, TOutput> Calculate(Frame<TInput> frame)
        {
            throw new NotImplementedException();
        }
    }

    public interface ICalculator<in TType>
    {
        void Calculate(IFrame<TType> frame);
    }

    public class CalculateThing : ICalculator<CalculateThing.IInput>
    {
        public interface IInput
        {
            int Quantity { get; }
            int Price { get; }
        }

        public void Calculate(IFrame<IInput> frame)
        {
            throw new NotImplementedException();
        }
    }

    public class CalculateOtherThing : ICalculator<CalculateOtherThing.IInput>
    {
        public interface IInput
        {
            int Quantity { get; }
            int Price { get; }
        }

        public void Calculate(IFrame<IInput> frame)
        {
            int[] prices = frame.GetData(t => t.Price);
            int[] quantities = frame.GetData(t => t.Quantity);
        }
    }

    public class NonCalculateThing : ICalculator<NonCalculateThing.IInput>
    {
        public interface IInput
        {
            int Quantity { get; }
            int Price { get; }
        }

        public void Calculate(IFrame<IInput> frame)
        {
            throw new NotImplementedException();
        }
    }

    public class Input :
        CalculateThing.IInput,
        CalculateOtherThing.IInput
    {
        public string PositionId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }

    public interface IFrame<out TType>
    {
        T[] GetData<T>(Func<TType, T> kk);
    }

    public class Frame
    {
        public static FrameDefinition<TIndex, TType> Define<TIndex, TType>(Expression<Func<TType, TIndex>> index)
        {
            string indexName = TerribleExpressionStuff.GetPropertyInfo(index).Name;

            var columnNames = typeof(TType)
                .GetProperties()
                .Select(p => p.Name)
                .Except(new[] { indexName })
                .ToArray();

            return new FrameDefinition<TIndex, TType>(indexName, columnNames);
        }
    }

    public class TerribleExpressionStuff
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            if (!(propertyLambda.Body is MemberExpression member))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));
            }

            var propInfo = member.Member as PropertyInfo;

            if (propInfo == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));
            }

            return propInfo;
        }
    }

    public class FrameDefinition<TIndex, TType>
    {
        private readonly string[] columnNames;

        public FrameDefinition(string indexName, string[] columnNames)
        {
            this.columnNames = columnNames;
            IndexName = indexName;
        }

        public string IndexName { get; }

        public IEnumerable<string> GetColumnNames()
        {
            return columnNames;
        }

        public MutableFrame<TIndex, TType> PopulateFromData(TType[] inputs)
        {
            var mutableFrame = new MutableFrame<TIndex, TType>(inputs.Length);

            return mutableFrame;
        }
    }

    public class Frame<TType> : IFrame<TType>
    {
        public void Add(TType[] inputs)
        {
        }

        public T[] GetData<T>(Func<TType, T> kk)
        {
            throw new NotImplementedException();
        }
    }

    public class MutableFrame<TIndex, TType>
    {
        public MutableFrame(int size)
        {
            Size = size;
        }

        public int Size { get; }
    }

    public static class SubFrame
    {
        public static void Define<TFrameIndex, TFrameType, TSubType>(FrameDefinition<TFrameIndex, TFrameType> definition, TSubType instance)
        {
            throw new NotImplementedException();
        }
    }

    public class Test
    {
        [Fact]
        public void ShouldCreateEmptyFrame()
        {
            var definition = Frame.Define<string, Input>(t => t.PositionId);

            var columns = definition.GetColumnNames().ToArray();

            Assert.Equal("PositionId", definition.IndexName);

            Assert.Equal(2, columns.Length);
            Assert.Contains("Price", columns);
            Assert.Contains("Quantity", columns);
            Assert.DoesNotContain("PositionId", columns);
        }

        [Fact]
        public void ShouldPopulateFrame()
        {
            var definition = Frame.Define<string, Input>(t => t.PositionId);

            var frame = definition.PopulateFromData(new[]
            {
                new Input { Price = 300, Quantity = 100 },
                new Input { Price = 300, Quantity = 100 }
            });

            Assert.Equal(2, frame.Size);
        }

        [Fact]
        public void ShouldGetSubFrame()
        {
            var definition = Frame.Define<string, Input>(t => t.PositionId);

            var frame = definition.PopulateFromData(new[]
            {
                new Input { Price = 300, Quantity = 100 },
                new Input { Price = 300, Quantity = 100 }
            });

            var kk = SubFrame.Define(definition, default(CalculateOtherThing.IInput));

            Assert.Equal(2, frame.Size);
        }
    }
}
