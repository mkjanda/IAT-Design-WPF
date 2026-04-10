using com.sun.org.apache.bcel.@internal.generic;
using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Domain;
using System.Collections.ObjectModel;

namespace IAT.Core.Domain
{
    public partial class Iat : ObservableObject
    {
        public ObservableCollection<Trial> Trials { get; } = new();
        public ObservableCollection<Block> Blocks { get; } = new();
        public ObservableCollection<Stimulus> Stimuli { get; } = new();
        public ObservableCollection<InstructionScreen> InstructionScreens { get; } = new();

        public ValidationResult ValidateEntireTest()
        {
            // 1. Every trial must be valid
            foreach (var trial in Trials)
            {
                var stimulus = GetStimulusById(trial.StimulusId);   // your lookup helper
                var result = trial.Validate(stimulus);
                if (!result.IsValid)
                    return result;   // fail fast on first error (you can collect all later if needed)
            }

            // 2. Stimulus reuse across blocks with different keying is allowed — but every stimulus must appear in at least one trial
            if (!Stimuli.Any(s => Trials.Any(t => t.StimulusId == s.Id)))
                return ValidationResult.Fail("Every stimulus must be used in at least one trial");
            foreach (var stimulus in Stimuli)
                if (!stimulus.Validate().IsValid) 
                    return ValidationResult.Fail($"Stimulus '{stimulus.Id}' is invalid: {stimulus.Validate().ErrorMessage}");


            if (InstructionScreens.Count == 0)
                return ValidationResult.Fail("At least one instruction screen is required");


            foreach (var instrucction in InstructionScreens)
                if (!instrucction.Validate().IsValid)
                    return ValidationResult.Fail($"Instruction screen '{instrucction.Id}' is invalid: {instrucction.Validate().ErrorMessage}");

            return ValidationResult.Success;
        }

        private readonly Dictionary<Guid, Stimulus> _stimulusCache = new();
        private readonly Dictionary<Guid, Trial> _trialCache = new();   
        private readonly Dictionary<Guid, InstructionScreen> _instructionCache = new();

        private Stimulus? GetStimulusById(Guid id) => Stimuli.FirstOrDefault(s => s.Id == id);
    }
}