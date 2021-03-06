﻿/******************************************************************************
 * Copyright (c) 2010 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Xml.Linq;
using ABB.SrcML;
namespace ABB.SrcML.Test
{
    [TestFixture]
    [Category("Build")]
    public class ExternalTests
    {
        [TestFixtureSetUp]
        public static void ExternalTestInitialize()
        {
            Directory.CreateDirectory("external");
            Directory.CreateDirectory("external_xml");

            File.WriteAllBytes("external\\fileWithBom.cpp", new byte[3] { 0xEF, 0xBB, 0xBF });

            File.WriteAllText("external\\ClassWithConstructor.java", String.Format(@"package external;{0}{0}class ClassWithConstructor{0}{{{0}	private int hidden = 0;{0}{0}	public Test(int value){0}	{{{0}		hidden = value;{0}	}}{0}{0}	public int foo (char a){0}	{{{0}		return (int) a;{0}	}}{0}}}", Environment.NewLine));

            File.WriteAllText(@"external\cpp_parsing_error.c", String.Format(@"int testcase(int x){0}{{{0}	if(x < 0){0}		printf(""x < 0\n"");{0}#if 1{0}	else{0}		printf(""x >= 0\n"");{0}#else{0}	else{0}		printf(""no really, x >= 0\n"");{0}#endif{0}	return x;{0}}}{0}", Environment.NewLine));

            File.WriteAllText("external\\MacroWithoutSemicolon.cpp", String.Format(@"if (exists) {{{0}	Py_BEGIN_ALLOW_THREADS{0}	fp = fopen(filename, ""r"" PY_STDIOTEXTMODE);{0}	Py_END_ALLOW_THREADS{0}{0}	if (fp == NULL) {{{0}		exists = 0;{0}	}}{0}}}", Environment.NewLine));

            File.WriteAllText("external\\DestructorWithIfStatement.cpp", String.Format(@"~Test(){0}{{{0}	if(0){0}	{{{0}	}}{0}}}", Environment.NewLine));

            File.WriteAllText("external\\MethodWithFunctionPointerParameters.cpp", String.Format(@"void foo(int (*a)(char i), char b){0}{{{0}}}", Environment.NewLine));
        }

        [TestFixtureTearDown]
        public static void SRCTestCleanup()
        {
            foreach (var file in Directory.GetFiles("external"))
            {
                File.Delete(file);
            }
            foreach (var file in Directory.GetFiles("external_xml"))
            {
                File.Delete(file);
            }
            Directory.Delete("external");
            Directory.Delete("external_xml");
        }

        [Test]
        public void FileWithBom()
        {
            var srcmlObject = new ABB.SrcML.SrcML();

            var doc = srcmlObject.GenerateSrcMLFromFile("external\\fileWithBom.cpp", "external_xml\\fileWithBom.xml");
        }

        [Test]
        public void JavaClassWithConstructor()
        {
            var srcmlObject = new Src2SrcMLRunner();

            var doc = srcmlObject.GenerateSrcMLFromFile("external\\ClassWithConstructor.java", "external_xml\\ClassWithConstructor.java.xml");
            XElement classBlock = null;

            classBlock = doc.FileUnits.First().Element(SRC.Class).Element(SRC.Block);

            Assert.AreEqual(1, classBlock.Elements(SRC.Function).Count(), srcmlObject.ApplicationDirectory);
        }

        [Test]
        public void DeclStmtWithTwoDecl()
        {
            var srcmlObject = new Src2SrcMLRunner();
            var source = "int x = 0, y = 2;";

            var xml = srcmlObject.GenerateSrcMLFromString(source);
            var element = XElement.Parse(xml);

            var decl = element.Element(SRC.DeclarationStatement).Element(SRC.Declaration);
            var nameCount = decl.Elements(SRC.Name).Count();
            var initCount = decl.Elements(SRC.Init).Count();
            Assert.AreEqual(2, nameCount, srcmlObject.ApplicationDirectory);
            Assert.AreEqual(2, initCount, srcmlObject.ApplicationDirectory);
        }

        [Test]
        public void FunctionWithElseInCpp()
        {
            var srcmlObject = new Src2SrcMLRunner();

            var doc = srcmlObject.GenerateSrcMLFromFile("external\\cpp_parsing_error.c", "external_xml\\cpp_parsing_error.c.xml");

            Assert.AreEqual(1, doc.FileUnits.First().Elements().Count(), srcmlObject.ApplicationDirectory);
        }

        [Test]
        public void MacroWithoutSemicolon()
        {
            var srcmlObject = new Src2SrcMLRunner();

            var doc = srcmlObject.GenerateSrcMLFromFile("external\\MacroWithoutSemicolon.cpp", "external_xml\\MacroWithoutSemicolon.cpp.xml");

            Assert.AreEqual(2, doc.FileUnits.First().Descendants(SRC.If).Count());
        }

        [Test]
        public void DestructorWithIfStatement()
        {
            var srcmlObject = new Src2SrcMLRunner();

            var doc = srcmlObject.GenerateSrcMLFromFile("external\\DestructorWithIfStatement.cpp", "external_xml\\DestructorWithIfStatement.cpp.xml");

            Assert.AreEqual(1, doc.FileUnits.First().Descendants(SRC.Destructor).Count());
        }

        [Test]
        public void MethodWithFunctionPointerAsParameter()
        {
            var srcmlObject = new Src2SrcMLRunner();

            var doc = srcmlObject.GenerateSrcMLFromFile("external\\MethodWithFunctionPointerParameters.cpp", "external_xml\\MethodWithFunctionPointerParameters.cpp.xml");

            Assert.AreEqual(2, doc.FileUnits.First().Element(SRC.Function).Element(SRC.ParameterList).Elements(SRC.Parameter).Count());
        }
    }
}
