﻿$if$ ($useXrmToolingClientUsing$ == 1)using Microsoft.Xrm.Tooling.Connector;$else$using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;$endif$
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Moq;
using NUnit.Framework;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Configuration;

namespace $rootnamespace$
{
    [TestFixture]
    public class $fileinputname$
    {
        #region Class Constructor
        private readonly string _namespaceClassAssembly;
        public $fileinputname$()
        {
            //[Namespace.class name, assembly name] for the class/assembly being tested
            //Namespace and class name can be found on the class file being tested
            //Assembly name can be found under the project properties on the Application tab
            _namespaceClassAssembly = "$fullclassname$" + ", " + "$assemblyname$";
        }
        #endregion
        #region Test SetUp and TearDown
        // Use ClassSetUp to run code before running the first test in the class
        [TestFixtureSetUp] public void ClassSetUp() { }

        // Use ClassTearDown to run code after all tests in a class have run
        [TestFixtureTearDown] public void ClassTearDown()  { }

        // Use TestSetUp to run code before running each test 
        [SetUp] public void TestSetUp() { }

        // Use TestTearDown to run code after each test has run
        [TearDown] public void TestTearDown() { }
        #endregion

        [Test]
        public void TestMethod1()
        {
            //Target
            Entity targetEntity = new Entity { LogicalName = "name", Id = Guid.NewGuid() };

            //Input parameters
            var inputs = new Dictionary<string, object> 
            {
                //{ "Input1", new EntityReference("entity", Guid.NewGuid()) },
                //{ "Input2", "test" }
            };

            //Expected value(s)
            const string expected = null;

            //Invoke the workflow
            var output = InvokeWorkflow(_namespaceClassAssembly, ref targetEntity, inputs);

            //Test(s)
            Assert.AreEqual(expected, null);
        }
        
        /// <summary>
        /// Invokes the workflow.
        /// </summary>
        /// <param name="name">Namespace.Class, Assembly</param>
        /// <param name="target">The target entity</param>
        /// <param name="inputs">The workflow input parameters</param>
        /// <returns>The workflow output parameters</returns>
        private static IDictionary<string, object> InvokeWorkflow(string name, ref Entity target, Dictionary<string, object> inputs)
        {
            var testClass = Activator.CreateInstance(Type.GetType(name)) as CodeActivity;

            var factoryMock = new Mock<IOrganizationServiceFactory>();
            var tracingServiceMock = new Mock<ITracingService>();
            var workflowContextMock = new Mock<IWorkflowContext>();

            IOrganizationService service = CreateOrganizationService();

            //Mock workflow Context
            var workflowUserId = Guid.NewGuid();
            var workflowCorrelationId = Guid.NewGuid();
            var workflowInitiatingUserId = Guid.NewGuid();

            //Workflow Context Mock
            workflowContextMock.Setup(t => t.InitiatingUserId).Returns(workflowInitiatingUserId);
            workflowContextMock.Setup(t => t.CorrelationId).Returns(workflowCorrelationId);
            workflowContextMock.Setup(t => t.UserId).Returns(workflowUserId);
            var workflowContext = workflowContextMock.Object;

            //Organization Service Factory Mock
            factoryMock.Setup(t => t.CreateOrganizationService(It.IsAny<Guid>())).Returns(service);
            var factory = factoryMock.Object;

            //Tracing Service - Content written appears in output
            tracingServiceMock.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>(MoqExtensions.WriteTrace);
            var tracingService = tracingServiceMock.Object;

            //Parameter Collection
            ParameterCollection inputParameters = new ParameterCollection { { "Target", target } };
            workflowContextMock.Setup(t => t.InputParameters).Returns(inputParameters);

            //Workflow Invoker
            var invoker = new WorkflowInvoker(testClass);
            invoker.Extensions.Add(() => tracingService);
            invoker.Extensions.Add(() => workflowContext);
            invoker.Extensions.Add(() => factory);

            return invoker.Invoke(inputs);
        }

        /// <summary>
        /// Creates the organization service from credentials in the App.config
        /// </summary>
        /// <returns>IOrganizationService</returns>
        private static IOrganizationService CreateOrganizationService()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString;
            if (connectionString.IndexOf("[orgname]", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new Exception("CRM connection string not set in app.config.");

            $if$ ($useXrmToolingClientUsing$ == 1)CrmServiceClient crmService = new CrmServiceClient(connectionString);				
            return crmService.OrganizationWebProxyClient ?? (IOrganizationService)crmService.OrganizationServiceProxy;$else$CrmConnection connection = CrmConnection.Parse(connectionString);               
            return new OrganizationService(connection);$endif$
        }
    }
}
