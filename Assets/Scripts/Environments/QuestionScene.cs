using System.Collections.Generic;
using UnityEngine;

public class QuestionScene : Environments
{
    [Header("Scene Specific Question Items")]
    [SerializeField] private List<Question> questions;

    public override void SetupEnvironment()
    {
        isQuestionScene = true;
        base.SetupEnvironment();
    }

    public List<Question> GetQuestions()
    {
        return questions;
    }
}
