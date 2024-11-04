
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;

namespace DTOMaker.Gentime.Tests
{
    public class TemplateProcessorTests
    {
        [Fact]
        public void Basic01_Create()
        {
            var processor = new TemplateProcessor();
        }

        [Fact]
        public async Task Basic02_ProcessEmpty()
        {
            var inputSource =
                """
                // empty
                """;
            var input = inputSource.Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            var language = Language_CSharp.Instance;
            var outerScope = new ModelScope_Empty();
            var processor = new TemplateProcessor();
            string[] processed = processor.ProcessTemplate(input, language, outerScope);
            string outputSource = string.Join(Environment.NewLine, processed);
            await Verifier.Verify(outputSource);
        }

        [Fact]
        public async Task Basic03_If()
        {
            var inputSource =
                """
                //##eval Count := 0
                //##eval Condition1 := true
                //##if Condition1
                // Condition1 is TRUE
                //##eval Count := Count + 2
                //##else
                // Condition1 is FALSE
                //##eval Count := Count + 1
                //##endif
                // Count is T_Count_
                """;
            var input = inputSource.Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            var language = Language_CSharp.Instance;
            var outerScope = new ModelScope_Empty();
            var processor = new TemplateProcessor();
            string[] processed = processor.ProcessTemplate(input, language, outerScope);
            string outputSource = string.Join(Environment.NewLine, processed);
            await Verifier.Verify(outputSource);
        }

        [Fact]
        public async Task Basic04_Elif_True()
        {
            var inputSource =
                """
                //##eval Chosen := ""
                //##eval Condition1 := false
                //##eval Condition2 := true
                //##if Condition1
                // Condition1 is TRUE
                //##eval Chosen := Chosen + "1"
                //##elif Condition2
                // Condition2 is TRUE
                //##eval Chosen := Chosen + "2"
                //##else
                // Neither are TRUE
                //##endif
                // Chosen is: T_Chosen_
                """;
            var input = inputSource.Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            var language = Language_CSharp.Instance;
            var outerScope = new ModelScope_Empty();
            var processor = new TemplateProcessor();
            string[] processed = processor.ProcessTemplate(input, language, outerScope);
            string outputSource = string.Join(Environment.NewLine, processed);
            await Verifier.Verify(outputSource);
        }

        [Fact]
        public async Task Basic05_Elif_False()
        {
            var inputSource =
                """
                //##eval Chosen := ""
                //##eval Condition1 := false
                //##eval Condition2 := false
                //##if Condition1
                // Condition1 is TRUE
                //##eval Chosen := Chosen + "1"
                //##elif Condition2
                // Condition2 is TRUE
                //##eval Chosen := Chosen + "2"
                //##else
                // Neither are TRUE
                //##endif
                // Chosen is: T_Chosen_
                """;
            var input = inputSource.Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            var language = Language_CSharp.Instance;
            var outerScope = new ModelScope_Empty();
            var processor = new TemplateProcessor();
            string[] processed = processor.ProcessTemplate(input, language, outerScope);
            string outputSource = string.Join(Environment.NewLine, processed);
            await Verifier.Verify(outputSource);
        }
    }
}
