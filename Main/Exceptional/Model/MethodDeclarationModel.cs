/// <copyright file="MethodDeclarationModel.cs" manufacturer="CodeGears">
///   Copyright (c) CodeGears. All rights reserved.
/// </copyright>

using System.Collections.Generic;
using CodeGears.ReSharper.Exceptional.Analyzers;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace CodeGears.ReSharper.Exceptional.Model
{
    /// <summary>Stores data about processed <see cref="IMethodDeclaration"/></summary>
    internal class MethodDeclarationModel : ModelBase, IBlockModel
    {
        public IMethodDeclaration MethodDeclaration { get; set; }
        public DocCommentBlockModel DocCommentBlockModel { get; private set; }
        public List<TryStatementModel> TryStatementModels { get; private set; }
        public List<ThrowStatementModel> ThrowStatementModels { get; private set; }
        public IBlockModel ParentBlock { get; set; }

        public bool CatchesException(IDeclaredType exception)
        {
            return false;
        }

        public IDeclaredType GetCatchedException()
        {
            return null;
        }

        public MethodDeclarationModel(IMethodDeclaration methodDeclaration) : base(null)
        {
            MethodDeclaration = methodDeclaration;
            TryStatementModels = new List<TryStatementModel>();
            ThrowStatementModels = new List<ThrowStatementModel>();
        }

        public void Initialize()
        {
            InitializeDocCommentBlockModel();
        }

        private void InitializeDocCommentBlockModel()
        {
            var docCommentBlockOwner = this.MethodDeclaration as IDocCommentBlockOwnerNode;
            if (docCommentBlockOwner == null) return;

            var docCommentBlockNode = docCommentBlockOwner.GetDocCommentBlockNode();
            if (docCommentBlockNode == null) return;
            
            this.DocCommentBlockModel = new DocCommentBlockModel(this, docCommentBlockNode);
        }

        public override void AssignHighlights(CSharpDaemonStageProcessBase process)
        {
            foreach (var tryStatementModel in this.TryStatementModels)
            {
                tryStatementModel.AssignHighlights(process);
            }

            foreach (var throwStatementModel in this.ThrowStatementModels)
            {
                throwStatementModel.AssignHighlights(process);
            }

            if (this.DocCommentBlockModel != null)
            {
                this.DocCommentBlockModel.AssignHighlights(process);
            }
        }

        public override void Accept(AnalyzerBase analyzerBase)
        {
            foreach (var tryStatementModel in this.TryStatementModels)
            {
                tryStatementModel.Accept(analyzerBase);
            }
            
            foreach (var throwStatementModel in this.ThrowStatementModels)
            {
                throwStatementModel.Accept(analyzerBase);
            }

            if (this.DocCommentBlockModel != null)
            {
                this.DocCommentBlockModel.Accept(analyzerBase);
            }
        }

        public void Add(TryStatementModel tryStatementModel)
        {
            this.TryStatementModels.Add(tryStatementModel);
            tryStatementModel.ParentBlock = this;
        }
    }
}