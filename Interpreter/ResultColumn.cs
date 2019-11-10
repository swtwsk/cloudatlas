using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class ResultColumn : Result
    {
        public override Result BinaryOperationTyped(BinaryOp op, ResultSingle right)
        {
            throw new System.NotImplementedException();
        }

        public override Result UnaryOperation(UnaryOp op)
        {
            throw new System.NotImplementedException();
        }

        protected override Result CallMe(BinaryOp op, Result left)
        {
            throw new System.NotImplementedException();
        }

        public override Value Value => throw new System.NotImplementedException();
        public override ValueList List => throw new System.NotImplementedException();
        public override ValueList Column => throw new System.NotImplementedException();
        public override Result FilterNulls()
        {
            throw new System.NotImplementedException();
        }

        public override Result First(int size)
        {
            throw new System.NotImplementedException();
        }

        public override Result Last(int size)
        {
            throw new System.NotImplementedException();
        }

        public override Result Random(int size)
        {
            throw new System.NotImplementedException();
        }

        public override Result ConvertTo(AttributeType to)
        {
            throw new System.NotImplementedException();
        }

        public override ResultSingle IsNull => throw new System.NotImplementedException();
        public override AttributeType Type => throw new System.NotImplementedException();
    }
}