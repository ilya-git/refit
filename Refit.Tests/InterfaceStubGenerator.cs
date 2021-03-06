﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

using Refit.Generator;

using Xunit;

using Task = System.Threading.Tasks.Task;
using VerifyCS = Refit.Tests.CSharpSourceGeneratorVerifier<Refit.Generator.InterfaceStubGenerator>;

namespace Refit.Tests
{
    public class InterfaceStubGeneratorTests
    {
        static readonly MetadataReference RefitAssembly = MetadataReference.CreateFromFile(
            typeof(GetAttribute).Assembly.Location,
            documentation: XmlDocumentationProvider.CreateFromFile(Path.ChangeExtension(typeof(GetAttribute).Assembly.Location, ".xml")));

        static readonly ReferenceAssemblies ReferenceAssemblies;

        static InterfaceStubGeneratorTests()
        {
#if NET5_0
            ReferenceAssemblies = ReferenceAssemblies.Net.Net50;
#else
            ReferenceAssemblies = ReferenceAssemblies.Default
                .AddPackages(ImmutableArray.Create(new PackageIdentity("System.Text.Json", "5.0.1")));
#endif

#if NET461
            ReferenceAssemblies = ReferenceAssemblies
                .AddAssemblies(ImmutableArray.Create("System.Web"))
                .AddPackages(ImmutableArray.Create(new PackageIdentity("System.Net.Http", "4.3.4")));
#endif
        }

        [Fact(Skip = "Generator in test issue")]
        public void GenerateInterfaceStubsSmokeTest()
        {
            var fixture = new InterfaceStubGenerator();

            var driver = CSharpGeneratorDriver.Create(fixture);


            var inputCompilation = CreateCompilation(
                IntegrationTestHelper.GetPath("RestService.cs"),
                IntegrationTestHelper.GetPath("GitHubApi.cs"),
                IntegrationTestHelper.GetPath("InheritedInterfacesApi.cs"),
                IntegrationTestHelper.GetPath("InheritedGenericInterfacesApi.cs"));

            var diags = inputCompilation.GetDiagnostics();

            // Make sure we don't have any errors
            Assert.Empty(diags.Where(d => d.Severity == DiagnosticSeverity.Error));

            var rundriver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompiliation, out var diagnostics);
            
            var runResult = rundriver.GetRunResult();

            var generated = runResult.Results[0];

            var text = generated.GeneratedSources.First().SourceText.ToString();

            Assert.Contains("IGitHubApi", text);
            Assert.Contains("IAmInterfaceC", text);
        }

        static Compilation CreateCompilation(params string[] sourceFiles)
        {
            var keyReferences = new[]
            {
                typeof(Binder),
                typeof(GetAttribute),
                typeof(RichardSzalay.MockHttp.MockHttpMessageHandler),
                typeof(System.Reactive.Unit),
                typeof(System.Linq.Enumerable),
                typeof(Newtonsoft.Json.JsonConvert),
                typeof(Xunit.FactAttribute),
                typeof(System.Net.Http.HttpContent),
                typeof(ModelObject),
                typeof(Attribute)
            };


            return CSharpCompilation.Create("compilation",
                sourceFiles.Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source))),
                                   keyReferences.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location)),
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        }

        [Fact]
        public async Task FindInterfacesSmokeTest()
        {
            var input = File.ReadAllText(IntegrationTestHelper.GetPath("GitHubApi.cs"));
            var output1 = @"
using System;
#pragma warning disable
namespace RefitInternalGenerated
{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    sealed class PreserveAttribute : Attribute
    {
        //
        // Fields
        //
        public bool AllMembers;

        public bool Conditional;
    }
}
#pragma warning restore
";
            var output2 = @"
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

namespace Refit.Tests
{
    /// <inheritdoc />
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::RefitInternalGenerated.PreserveAttribute]
    [global::System.Reflection.Obfuscation(Exclude=true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    partial class AutoGeneratedIGitHubApi
        : global::Refit.Tests.IGitHubApi

    {
        /// <inheritdoc />
        public global::System.Net.Http.HttpClient Client { get; }
        readonly global::Refit.IRequestBuilder requestBuilder;

        /// <inheritdoc />
        public AutoGeneratedIGitHubApi(global::System.Net.Http.HttpClient client, global::Refit.IRequestBuilder requestBuilder)
        {
            Client = client;
            this.requestBuilder = requestBuilder;
        }
    


        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.Tests.User> global::Refit.Tests.IGitHubApi.GetUser(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUser"", new global::System.Type[] { typeof(string) } );
            return (global::System.Threading.Tasks.Task<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.IObservable<global::Refit.Tests.User> global::Refit.Tests.IGitHubApi.GetUserObservable(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUserObservable"", new global::System.Type[] { typeof(string) } );
            return (global::System.IObservable<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.IObservable<global::Refit.Tests.User> global::Refit.Tests.IGitHubApi.GetUserCamelCase(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUserCamelCase"", new global::System.Type[] { typeof(string) } );
            return (global::System.IObservable<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::System.Collections.Generic.List<global::Refit.Tests.User>> global::Refit.Tests.IGitHubApi.GetOrgMembers(string @orgName, global::System.Threading.CancellationToken @cancellationToken) 
        {
            var arguments = new object[] { @orgName, @cancellationToken };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetOrgMembers"", new global::System.Type[] { typeof(string), typeof(global::System.Threading.CancellationToken) } );
            return (global::System.Threading.Tasks.Task<global::System.Collections.Generic.List<global::Refit.Tests.User>>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.Tests.UserSearchResult> global::Refit.Tests.IGitHubApi.FindUsers(string @q) 
        {
            var arguments = new object[] { @q };
            var func = requestBuilder.BuildRestResultFuncForMethod(""FindUsers"", new global::System.Type[] { typeof(string) } );
            return (global::System.Threading.Tasks.Task<global::Refit.Tests.UserSearchResult>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::System.Net.Http.HttpResponseMessage> global::Refit.Tests.IGitHubApi.GetIndex() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetIndex"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task<global::System.Net.Http.HttpResponseMessage>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.IObservable<string> global::Refit.Tests.IGitHubApi.GetIndexObservable() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetIndexObservable"", new global::System.Type[] {  } );
            return (global::System.IObservable<string>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.Tests.User> global::Refit.Tests.IGitHubApi.NothingToSeeHere() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""NothingToSeeHere"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.ApiResponse<global::Refit.Tests.User>> global::Refit.Tests.IGitHubApi.NothingToSeeHereWithMetadata() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""NothingToSeeHereWithMetadata"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task<global::Refit.ApiResponse<global::Refit.Tests.User>>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.ApiResponse<global::Refit.Tests.User>> global::Refit.Tests.IGitHubApi.GetUserWithMetadata(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUserWithMetadata"", new global::System.Type[] { typeof(string) } );
            return (global::System.Threading.Tasks.Task<global::Refit.ApiResponse<global::Refit.Tests.User>>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.IObservable<global::Refit.ApiResponse<global::Refit.Tests.User>> global::Refit.Tests.IGitHubApi.GetUserObservableWithMetadata(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUserObservableWithMetadata"", new global::System.Type[] { typeof(string) } );
            return (global::System.IObservable<global::Refit.ApiResponse<global::Refit.Tests.User>>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.Tests.User> global::Refit.Tests.IGitHubApi.CreateUser(global::Refit.Tests.User @user) 
        {
            var arguments = new object[] { @user };
            var func = requestBuilder.BuildRestResultFuncForMethod(""CreateUser"", new global::System.Type[] { typeof(global::Refit.Tests.User) } );
            return (global::System.Threading.Tasks.Task<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.ApiResponse<global::Refit.Tests.User>> global::Refit.Tests.IGitHubApi.CreateUserWithMetadata(global::Refit.Tests.User @user) 
        {
            var arguments = new object[] { @user };
            var func = requestBuilder.BuildRestResultFuncForMethod(""CreateUserWithMetadata"", new global::System.Type[] { typeof(global::Refit.Tests.User) } );
            return (global::System.Threading.Tasks.Task<global::Refit.ApiResponse<global::Refit.Tests.User>>)func(Client, arguments);
        }
    }
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
";
            var output3 = @"
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

namespace Refit.Tests
{
    /// <inheritdoc />
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::RefitInternalGenerated.PreserveAttribute]
    [global::System.Reflection.Obfuscation(Exclude=true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    partial class AutoGeneratedIGitHubApiDisposable
        : global::Refit.Tests.IGitHubApiDisposable

    {
        /// <inheritdoc />
        public global::System.Net.Http.HttpClient Client { get; }
        readonly global::Refit.IRequestBuilder requestBuilder;

        /// <inheritdoc />
        public AutoGeneratedIGitHubApiDisposable(global::System.Net.Http.HttpClient client, global::Refit.IRequestBuilder requestBuilder)
        {
            Client = client;
            this.requestBuilder = requestBuilder;
        }
    


        /// <inheritdoc />
        global::System.Threading.Tasks.Task global::Refit.Tests.IGitHubApiDisposable.RefitMethod() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""RefitMethod"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task)func(Client, arguments);
        }

        /// <inheritdoc />
        void global::System.IDisposable.Dispose() 
        {
                Client?.Dispose();
        }
    }
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
";
            var output4 = @"
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

namespace Refit.Tests
{
    /// <inheritdoc />
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::RefitInternalGenerated.PreserveAttribute]
    [global::System.Reflection.Obfuscation(Exclude=true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    partial class AutoGeneratedTestNestedINestedGitHubApi
        : global::Refit.Tests.TestNested.INestedGitHubApi

    {
        /// <inheritdoc />
        public global::System.Net.Http.HttpClient Client { get; }
        readonly global::Refit.IRequestBuilder requestBuilder;

        /// <inheritdoc />
        public AutoGeneratedTestNestedINestedGitHubApi(global::System.Net.Http.HttpClient client, global::Refit.IRequestBuilder requestBuilder)
        {
            Client = client;
            this.requestBuilder = requestBuilder;
        }
    


        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.Tests.User> global::Refit.Tests.TestNested.INestedGitHubApi.GetUser(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUser"", new global::System.Type[] { typeof(string) } );
            return (global::System.Threading.Tasks.Task<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.IObservable<global::Refit.Tests.User> global::Refit.Tests.TestNested.INestedGitHubApi.GetUserObservable(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUserObservable"", new global::System.Type[] { typeof(string) } );
            return (global::System.IObservable<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.IObservable<global::Refit.Tests.User> global::Refit.Tests.TestNested.INestedGitHubApi.GetUserCamelCase(string @userName) 
        {
            var arguments = new object[] { @userName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetUserCamelCase"", new global::System.Type[] { typeof(string) } );
            return (global::System.IObservable<global::Refit.Tests.User>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::System.Collections.Generic.List<global::Refit.Tests.User>> global::Refit.Tests.TestNested.INestedGitHubApi.GetOrgMembers(string @orgName) 
        {
            var arguments = new object[] { @orgName };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetOrgMembers"", new global::System.Type[] { typeof(string) } );
            return (global::System.Threading.Tasks.Task<global::System.Collections.Generic.List<global::Refit.Tests.User>>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::Refit.Tests.UserSearchResult> global::Refit.Tests.TestNested.INestedGitHubApi.FindUsers(string @q) 
        {
            var arguments = new object[] { @q };
            var func = requestBuilder.BuildRestResultFuncForMethod(""FindUsers"", new global::System.Type[] { typeof(string) } );
            return (global::System.Threading.Tasks.Task<global::Refit.Tests.UserSearchResult>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task<global::System.Net.Http.HttpResponseMessage> global::Refit.Tests.TestNested.INestedGitHubApi.GetIndex() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetIndex"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task<global::System.Net.Http.HttpResponseMessage>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.IObservable<string> global::Refit.Tests.TestNested.INestedGitHubApi.GetIndexObservable() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetIndexObservable"", new global::System.Type[] {  } );
            return (global::System.IObservable<string>)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task global::Refit.Tests.TestNested.INestedGitHubApi.NothingToSeeHere() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""NothingToSeeHere"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task)func(Client, arguments);
        }
    }
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
";

            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestState =
                {
                    AdditionalReferences = { RefitAssembly },
                    Sources = { input },
                },
                FixedState =
                {
                    Sources =
                    {
                        input,
                        (@"InterfaceStubGenerator.Core\Refit.Generator.InterfaceStubGenerator\PreserveAttribute.cs", SourceText.From(output1, Encoding.UTF8)),
                        (@"InterfaceStubGenerator.Core\Refit.Generator.InterfaceStubGenerator\IGitHubApi_refit.cs", SourceText.From(output2, Encoding.UTF8)),
                        (@"InterfaceStubGenerator.Core\Refit.Generator.InterfaceStubGenerator\IGitHubApiDisposable_refit.cs", SourceText.From(output3, Encoding.UTF8)),
                        (@"InterfaceStubGenerator.Core\Refit.Generator.InterfaceStubGenerator\INestedGitHubApi_refit.cs", SourceText.From(output4, Encoding.UTF8)),
                    },
                },
            }.RunAsync();
        }
     

        [Fact]
        public async Task GenerateInterfaceStubsWithoutNamespaceSmokeTest()
        {
            var input = File.ReadAllText(IntegrationTestHelper.GetPath("IServiceWithoutNamespace.cs"));
            var output1 = @"
using System;
#pragma warning disable
namespace RefitInternalGenerated
{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    sealed class PreserveAttribute : Attribute
    {
        //
        // Fields
        //
        public bool AllMembers;

        public bool Conditional;
    }
}
#pragma warning restore
";
            var output2 = @"
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

namespace AutoGeneratedIServiceWithoutNamespace
{
    /// <inheritdoc />
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::RefitInternalGenerated.PreserveAttribute]
    [global::System.Reflection.Obfuscation(Exclude=true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    partial class AutoGeneratedIServiceWithoutNamespace
        : global::IServiceWithoutNamespace

    {
        /// <inheritdoc />
        public global::System.Net.Http.HttpClient Client { get; }
        readonly global::Refit.IRequestBuilder requestBuilder;

        /// <inheritdoc />
        public AutoGeneratedIServiceWithoutNamespace(global::System.Net.Http.HttpClient client, global::Refit.IRequestBuilder requestBuilder)
        {
            Client = client;
            this.requestBuilder = requestBuilder;
        }
    


        /// <inheritdoc />
        global::System.Threading.Tasks.Task global::IServiceWithoutNamespace.GetRoot() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""GetRoot"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task)func(Client, arguments);
        }

        /// <inheritdoc />
        global::System.Threading.Tasks.Task global::IServiceWithoutNamespace.PostRoot() 
        {
            var arguments = new object[] {  };
            var func = requestBuilder.BuildRestResultFuncForMethod(""PostRoot"", new global::System.Type[] {  } );
            return (global::System.Threading.Tasks.Task)func(Client, arguments);
        }
    }
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
";

            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies,
                TestState =
                {
                    AdditionalReferences = { RefitAssembly },
                    Sources = { input },
                },
                FixedState =
                {
                    Sources =
                    {
                        input,
                        (@"InterfaceStubGenerator.Core\Refit.Generator.InterfaceStubGenerator\PreserveAttribute.cs", SourceText.From(output1, Encoding.UTF8)),
                        (@"InterfaceStubGenerator.Core\Refit.Generator.InterfaceStubGenerator\IServiceWithoutNamespace_refit.cs", SourceText.From(output2, Encoding.UTF8)),
                    },
                },
            }.RunAsync();
        }
    }

    public static class ThisIsDumbButMightHappen
    {
        public const string PeopleDoWeirdStuff = "But we don't let them";
    }

    public interface IAmARefitInterfaceButNobodyUsesMe
    {
        [Get("whatever")]
        Task RefitMethod();

        [Refit.GetAttribute("something-else")]
        Task AnotherRefitMethod();

        [Get(ThisIsDumbButMightHappen.PeopleDoWeirdStuff)]
        Task NoConstantsAllowed();

        [Get("spaces-shouldnt-break-me")]
        Task SpacesShouldntBreakMe();

        // We don't need an explicit test for this because if it isn't supported we can't compile
        [Get("anything")]
        Task ReservedWordsForParameterNames(int @int, string @string, float @long);
    }

    public interface IAmNotARefitInterface
    {
        Task NotARefitMethod();
    }

    public interface IBoringCrudApi<T, in TKey> where T : class
    {
        [Post("")]
        Task<T> Create([Body] T paylod);

        [Get("")]
        Task<List<T>> ReadAll();

        [Get("/{key}")]
        Task<T> ReadOne(TKey key);

        [Put("/{key}")]
        Task Update(TKey key, [Body]T payload);

        [Delete("/{key}")]
        Task Delete(TKey key);
    }

    public interface INonGenericInterfaceWithGenericMethod
    {
        [Post("")]
        Task PostMessage<T>([Body] T message) where T : IMessage;

        [Post("")]
        Task PostMessage<T, U, V>([Body] T message, U param1, V param2) where T : IMessage where U : T;
    }

    public interface IMessage { }

}
