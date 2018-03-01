using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using System;
using System.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Woodman.Alexa.NextYoullSay
{
    public class Function
    {
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            try
            {
                return new SkillResponse
                {
                    Version = "1.0",
                    Response = GetResponse(input, context)
                };
            }
            catch (Exception e)
            {
                context.Logger.Log($"Error: {e.Message}");

                return new SkillResponse
                {
                    Version = "1.0",
                    Response = new ResponseBody
                    {
                        ShouldEndSession = false,
                        OutputSpeech = new PlainTextOutputSpeech
                        {
                            Text = "I'm sorry, I didn't undertand. Try saying, Your next line, is this is fun."
                        }
                    }
                };
            }
        }

        private static ResponseBody GetResponse(SkillRequest input, ILambdaContext context)
        {
            var responseBody = new ResponseBody
            {
                ShouldEndSession = false
            };

            context.Logger.LogLine($"{input.Request.GetType().Name} made.");

            if(input?.Request == null)
            {
                HandleError(responseBody);
            }
            if (input.Request is LaunchRequest launchRequest)
            {
                responseBody.OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = "Welcome to JoJo Time. Try saying: Next you'll say, this is fun."
                };
            }
            else if(input.Request is SessionEndedRequest)
            {
                responseBody.ShouldEndSession = true;
                responseBody.OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = "See you later."
                };
            }
            else if(input.Request is IntentRequest intentRequest)
            {
                HandleIntentRequest(responseBody, intentRequest);
            }
            else
            {
                HandleError(responseBody);
            }

            return responseBody;
        }

        private static void HandleError(ResponseBody responseBody)
        {
            responseBody.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = "I'm sorry, I didn't undertand. Try saying, Your next line, is this is fun."
            };
        }

        private static void HandleIntentRequest(ResponseBody responseBody, IntentRequest intentRequest)
        {
            if (intentRequest.Intent.Name == BuiltInIntent.Stop)
            {
                responseBody.ShouldEndSession = true;
                responseBody.OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = "See you later."
                };
            }
            else if (intentRequest.Intent.Name == BuiltInIntent.Cancel)
            {
                responseBody.ShouldEndSession = true;
                responseBody.OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = "See you later."
                };
            }
            else if (intentRequest.Intent.Name == BuiltInIntent.Help)
            {
                responseBody.OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = "I heard you need some help. Try saying, Your next line is, this is fun."
                };
            }
            else
            {
                var slot = intentRequest?.Intent?.Slots?.Values?.FirstOrDefault();

                var response = string.IsNullOrWhiteSpace(slot?.Value ?? string.Empty)
                    ? "I'm sorry, I didn't undertand. Try saying, Your next line, is this is fun."
                    : slot.Value;

                var rand = new Random().Next(1, 100);

                responseBody.OutputSpeech = new SsmlOutputSpeech
                {
                    Ssml = rand % 13 == 0
                        ? $"<speak><audio src=\"{AudioClips.Muda}\"/></speak>"
                        : rand % 10 == 0
                            ? $"<speak>{response}<audio src=\"{AudioClips.Scream}\"/></speak>"
                            : $"<speak>{response}</speak>"
                };
            }
        }

        private static string GetSsml(string input, bool useAudio, string audioLink)
        {
            return useAudio 
                ? $"<speak>{input}<audio src=\"{audioLink}\"/></speak>"
                : $"<speak>{input}</speak>";
        }

        private static class AudioClips
        {
            public static string Scream = "https://s3-us-west-2.amazonaws.com/jw-sounds/scream.mp3";
            public static string Muda = "https://s3-us-west-2.amazonaws.com/jw-sounds/muda.mp3";
        }
    }
}
