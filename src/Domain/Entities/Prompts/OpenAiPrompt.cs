using System;
using System.Collections.Generic;

namespace Domain.Entities.Prompts;

public class OpenAiPrompt : HasDatesBase
{
    public const string DefaultCompanyAnalyzePrompt =
        "You are a helpful career assistant. " +
        "Analyze the company's reviews and provide " +
        "a summary with advice what should user pay more attention on. " +
        "In the request there will be a company total rating, rating history and reviews presented in JSON format. Your reply should be in Russian language, markdown formatted.";

    public const string DefaultChatAnalyzePrompt =
        "You are a helpful assistant. Analyze the user's input and provide a response. " +
        "Your reply should be in question language, markdown formatted.";

    public Guid Id { get; protected set; }

    public OpenAiPromptType Type { get; protected set; }

    public string Prompt { get; protected set; }

    public string Model { get; protected set; }

    public AiEngine Engine { get; protected set; }

    public bool IsActive { get; protected set; }

    public OpenAiPrompt(
        OpenAiPromptType id,
        string prompt,
        string model,
        AiEngine engine)
    {
        Id = Guid.NewGuid();
        Type = id;
        Prompt = prompt?.Trim();
        Model = model?.Trim().ToLowerInvariant();
        Engine = engine;
        IsActive = false;
    }

    protected OpenAiPrompt()
    {
    }

    public void Update(
        string prompt,
        string model,
        AiEngine engine)
    {
        prompt = prompt?.Trim();
        if (string.IsNullOrEmpty(prompt))
        {
            throw new InvalidOperationException("Prompt cannot be null or empty.");
        }

        model = model?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(model))
        {
            throw new InvalidOperationException("Model cannot be null or empty.");
        }

        Prompt = prompt;
        Model = model;
        Engine = engine;
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new InvalidOperationException("Already active.");
        }

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Already inactive.");
        }

        IsActive = false;
    }
}