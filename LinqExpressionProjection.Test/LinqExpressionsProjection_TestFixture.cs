﻿using System;
using System.Linq.Expressions;
using System.Linq;
using LinqExpressionProjection.Test.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqExpressionProjection.Test
{
    [TestClass]
    public class LinqExpressionsProjection_TestFixture
    {
        private static readonly Expression<Func<Project, double>> ProjectAverageEffectiveAreaSelectorStatic =
            proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);

        private readonly Expression<Func<Project, double>> _projectAverageEffectiveAreaSelector =
            proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);

        public static Expression<Func<Project, double>> GetProjectAverageEffectiveAreaSelectorStatic()
        {
            return proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);
        }

        public Expression<Func<Project, double>> GetProjectAverageEffectiveAreaSelector()
        {
            return proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);
        }

        public Expression<Func<Project, double>> GetProjectAverageEffectiveAreaSelectorWithLogic(bool isOverThousandIncluded = false)
        {
            return isOverThousandIncluded
            ? (Expression<Func<Project, double>>) ((Project proj) => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area))
            : (Project proj) => proj.Subprojects.Average(sp => sp.Area);
        }

        [ClassInitialize]
        public static void RunBeforeAnyTests(TestContext testContext)
        {
            ClearDb();
            PopulateDb();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ProjectingExpressionFailsOnNormalCases_Test()
        {
            Expression<Func<Project, double>> localSelector =
                proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);
            using (var ctx = new ProjectsDbContext())
            {
                var v = (from p in ctx.Projects
                         select new
                         {
                             Project = p,
                             AEA = localSelector
                         }).ToArray();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ProjectingExpressionFailsWithNoCallToAsExpressionProjectable_Test()
        {
            Expression<Func<Project, double>> localSelector =
                proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects
                                select new
                                {
                                    Project = p,
                                    AEA = localSelector.Project<double>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProjectingExpressionFailsWithProjectionNotMatchingLambdaReturnType_Test()
        {
            Expression<Func<Project, double>> localSelector =
                proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                {
                                    Project = p,
                                    AEA = localSelector.Project<int>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProjectingExpressionFailsWithWrongLambdaParameterType_Test()
        {
            Expression<Func<Subproject, double>> localSelector =
                sp => sp.Area;
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                {
                                    Project = p,
                                    AEA = localSelector.Project<double>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        public void ProjectingExpressionByLocalVariable_Test()
        {
            Expression<Func<Project, double>> localSelector =
                proj => proj.Subprojects.Where(sp => sp.Area < 1000).Average(sp => sp.Area);
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                           {
                                               Project = p,
                                               AEA = localSelector.Project<double>()
                                           }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        public void ProjectingExpressionByStaticField_Test()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                           {
                                               Project = p,
                                               AEA = ProjectAverageEffectiveAreaSelectorStatic.Project<double>()
                                           }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }


        [TestMethod]
        public void ProjectingExpressionByNonStaticField_Test()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                {
                                    Project = p,
                                    AEA = _projectAverageEffectiveAreaSelector.Project<double>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        public void ProjectingExpressionByStaticMethod_Test()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                {
                                    Project = p,
                                    AEA = GetProjectAverageEffectiveAreaSelectorStatic().Project<double>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        public void ProjectingExpressionByNonStaticMethod_Test()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                {
                                    Project = p,
                                    AEA = GetProjectAverageEffectiveAreaSelector().Project<double>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        public void ProjectingExpressionByNonStaticMethodWithLogic_Test()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                {
                                    Project = p,
                                    AEA = GetProjectAverageEffectiveAreaSelectorWithLogic(false).Project<double>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(3600, projects[1].AEA);
            }
            using (var ctx = new ProjectsDbContext())
            {
                var projects = (from p in ctx.Projects.AsExpressionProjectable()
                                select new
                                {
                                    Project = p,
                                    AEA = GetProjectAverageEffectiveAreaSelectorWithLogic(true).Project<double>()
                                }).ToArray();
                Assert.AreEqual(150, projects[0].AEA);
                Assert.AreEqual(400, projects[1].AEA);
            }
        }

        [TestMethod]
        public void ValidateAndLog_ProjectsDbContext_Connection()
        {
            using (var ctx = new ProjectsDbContext())
            {
                Console.WriteLine(ctx.Database.Connection.ConnectionString);
            }
        }

        [TestMethod]
        public void Can_Project_StaticField_BasicExpression()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var subprojects = ctx.Subprojects.AsExpressionProjectable().Select(
                    subproject => new
                                  {
                                      subproject,
                                      testResult = Subproject.StaticFieldOnType_BasicExpression.Project<string>()
                                  }).ToArray();

                Assert.AreEqual("StaticFieldOnType_BasicExpression - Area: 100", subprojects.ElementAt(0).testResult);
                Assert.AreEqual("StaticFieldOnType_BasicExpression - Area: 450", subprojects.ElementAt(3).testResult);
            }
        }

        // rs-todo: investigate whether can get this working - currently throws error: 
        // System.InvalidOperationException: variable 'subproject' of type 'LinqExpressionProjection.Test.Model.Subproject' referenced from scope '', but it is not defined
        //public class Subproject
        //{
        //    ...
        //    public readonly Expression<Func<Subproject, string>> BasicMemberFieldExpression = subProject => "Area: " + subProject.Area;
        //    ...
        //}
        // possibly use Expression.Parameter(entityType, "entity"), but we don't know those details at compile time
        //[TestMethod]
        //public void Can_Project_Basic_MemberFieldExpression()
        //{
        //    using (var ctx = new ProjectsDbContext())
        //    {
        //        var subprojects = ctx.Subprojects.AsExpressionProjectable().Select(
        //            subproject => new
        //            {
        //                subproject,
        //                testResult = subproject.BasicMemberFieldExpression.Project2(subproject)
        //            }).ToArray();
        //
        //        Assert.AreEqual("BasicStaticFieldExpression - Area: 100", subprojects.ElementAt(0).testResult);
        //        Assert.AreEqual("BasicStaticFieldExpression - Area: 450", subprojects.ElementAt(3).testResult);
        //    }
        //}

        private static class TestExpressions
        {
            public static readonly Expression<Func<Subproject, string>> BasicMemberExpression = subProject => "Area: " + subProject.Area;

            public static readonly Expression<Func<Project, string>> MemberOfMemberExpression = project => "Subprojects Count: " + project.Subprojects.Count;
        }

        [TestMethod]
        public void Can_Project_BasicMemberExpression()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var subprojects = ctx.Subprojects.AsExpressionProjectable().Select(
                    subproject => new
                                  {
                                      subproject,
                                      testResult = TestExpressions.BasicMemberExpression.Project2(subproject)
                                  }).ToArray();

                Assert.AreEqual("Area: 100", subprojects.ElementAt(0).testResult);
                Assert.AreEqual("Area: 450", subprojects.ElementAt(3).testResult);
            }
        }

        [TestMethod]
        public void Can_Project_MemberOfMemberExpression()
        {
            using (var ctx = new ProjectsDbContext())
            {
                var subprojects = ctx.Subprojects.AsExpressionProjectable().Select(
                    subproject => new
                    {
                        subproject,
                        testResult = TestExpressions.MemberOfMemberExpression.Project2(subproject.Project)
                    }).ToArray();

                Assert.AreEqual("Subprojects Count: 2", subprojects.ElementAt(0).testResult);
                Assert.AreEqual("Subprojects Count: 2", subprojects.ElementAt(1).testResult);
                Assert.AreEqual("Subprojects Count: 3", subprojects.ElementAt(2).testResult);
                Assert.AreEqual("Subprojects Count: 3", subprojects.ElementAt(3).testResult);
                Assert.AreEqual("Subprojects Count: 3", subprojects.ElementAt(4).testResult);
            }
        }

        [TestMethod]
        public void Can_Project_MemberExpressionOfMainLambdaParameter()
        {
            Expression<Func<User, string>> projectionExpression = user => user.Name + "-somepostfix";

            using (var ctx = new ProjectsDbContext())
            {
                var projects = ctx.Projects.AsExpressionProjectable().Select(
                    project => new
                               {
                                   testResult1 = projectionExpression.Project2(project.CreatedBy),
                                   testResult2 = projectionExpression.Project2(project.ModifiedBy)
                               }).ToArray();

                Assert.AreEqual("user1-somepostfix", projects.ElementAt(0).testResult1);
                Assert.AreEqual("user3-somepostfix", projects.ElementAt(0).testResult2);

                Assert.AreEqual("user2-somepostfix", projects.ElementAt(1).testResult1);
                Assert.AreEqual("user4-somepostfix", projects.ElementAt(1).testResult2);
            }
        }

        [TestMethod]
        public void Can_Project_InnerExpression()
        {
            Expression<Func<string, string>> projectionExpression = s => s + "-somepostfix";

            using (var ctx = new ProjectsDbContext())
            {
                var projects = ctx.Projects.AsExpressionProjectable().Select(
                    project => new
                               {
                                   testResult1 = projectionExpression.Project2("hello world"),
                                   testResult2 = projectionExpression.Project2((2 + 10).ToString())
                               }).ToArray();

                Assert.AreEqual("hello world-somepostfix", projects.ElementAt(0).testResult1);
                Assert.AreEqual("12-somepostfix", projects.ElementAt(0).testResult2);

                Assert.AreEqual("hello world-somepostfix", projects.ElementAt(1).testResult1);
                Assert.AreEqual("12-somepostfix", projects.ElementAt(1).testResult2);
            }
        }

        private static void ClearDb()
        {
            using (var ctx = new ProjectsDbContext())
            {
                foreach (var subproject in ctx.Subprojects)
                {
                    ctx.Subprojects.Remove(subproject);
                }
                foreach (var project in ctx.Projects)
                {
                    ctx.Projects.Remove(project);
                }
                foreach (var user in ctx.Users)
                {
                    ctx.Users.Remove(user);
                }
                ctx.SaveChanges();
            }
        }

        private static void PopulateDb()
        {
            using (var ctx = new ProjectsDbContext())
            {
                User user1 = ctx.Users.Add(new User { Name = "user1" });
                User user2 = ctx.Users.Add(new User { Name = "user2" });
                User user3 = ctx.Users.Add(new User { Name = "user3" });
                User user4 = ctx.Users.Add(new User { Name = "user4" });

                ctx.SaveChanges();

                Project p1 = ctx.Projects.Add(new Project { CreatedBy = user1, ModifiedBy = user3 });
                Project p2 = ctx.Projects.Add(new Project { CreatedBy = user2, ModifiedBy = user4 });

                ctx.Subprojects.Add(new Subproject { Area = 100, Project = p1 });
                ctx.Subprojects.Add(new Subproject { Area = 200, Project = p1 });
                ctx.Subprojects.Add(new Subproject { Area = 350, Project = p2 });
                ctx.Subprojects.Add(new Subproject { Area = 450, Project = p2 });
                ctx.Subprojects.Add(new Subproject { Area = 10000, Project = p2 });

                ctx.SaveChanges();
            }
        }
    }
}
