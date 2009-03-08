using System.Collections.Generic;
using CodeGears.ReSharper.Exceptional.Analyzers;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace CodeGears.ReSharper.Exceptional.Model
{
    internal class TryStatementModel : ModelBase, IBlockModel
    {
        public ITryStatement TryStatement { get; private set; }
        
        public List<CatchClauseModel> CatchClauseModels { get; private set; }
        public List<ThrowStatementModel> ThrowStatementModels { get; private set; }
        public List<TryStatementModel> TryStatementModels { get; private set; }
        public IBlockModel ParentBlock { get; set; }

        public TryStatementModel(MethodDeclarationModel methodDeclarationModel, ITryStatement tryStatement)
            : base(methodDeclarationModel)
        {
            TryStatement = tryStatement;
            CatchClauseModels = new List<CatchClauseModel>();
            ThrowStatementModels = new List<ThrowStatementModel>();
            TryStatementModels = new List<TryStatementModel>();
        }

        public bool CatchesException(IDeclaredType exception)
        {
            foreach (var catchClauseModel in this.CatchClauseModels)
            {
                if(catchClauseModel.Catches(exception))
                {
                    return true;
                }
            }

            return this.ParentBlock.CatchesException(exception);
        }

        public IDeclaredType GetCatchedException()
        {
            return this.ParentBlock.GetCatchedException();
        }

        public IEnumerable<ThrowStatementModel> ThrowStatementModelsNotCatched
        {
            get
            {
                foreach (var throwStatementModel in this.ThrowStatementModels)
                {
                    if(throwStatementModel.IsCatched == false)
                    {
                        yield return throwStatementModel;
                    }
                }

                for (var i = 0; i < this.TryStatementModels.Count; i++)
                {
                    IBlockModel tryStatementModel = this.TryStatementModels[i];
                    foreach (var model in tryStatementModel.ThrowStatementModelsNotCatched)
                    {
                        yield return model;
                    }
                }

                for (var i = 0; i < this.CatchClauseModels.Count; i++)
                {
                    IBlockModel catchClauseModel = this.CatchClauseModels[i];
                    foreach (var model in catchClauseModel.ThrowStatementModelsNotCatched)
                    {
                        yield return model;
                    }
                }
            }
        }

        public override void Accept(AnalyzerBase analyzerBase)
        {
            analyzerBase.Visit(this);

            foreach (var innerTryStatementModel in this.TryStatementModels)
            {
                innerTryStatementModel.Accept(analyzerBase);
            }

            foreach (var catchClauseModel in this.CatchClauseModels)
            {
                catchClauseModel.Accept(analyzerBase);
            }

            foreach (var throwStatementModel in this.ThrowStatementModels)
            {
                throwStatementModel.Accept(analyzerBase);
            }
        }
    }
}