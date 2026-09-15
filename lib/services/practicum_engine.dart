import '../models/practicum.dart';
import '../models/practicum_step.dart';

class PracticumEngine {
  final Practicum practicum;

  int currentStep = 0;

  PracticumEngine(this.practicum);

  PracticumStep get current => practicum.steps[currentStep];

  bool validate(String itemId) {
    return current.requiredItemId == itemId;
  }

  bool next() {
    if (currentStep < practicum.steps.length - 1) {
      currentStep++;
      return true;
    }
    return false;
  }

  bool previous() {
    if (currentStep > 0) {
      currentStep--;
      return true;
    }
    return false;
  }

  bool get isLastStep {
    return currentStep == practicum.steps.length - 1;
  }

  double get progress {
    return (currentStep + 1) / practicum.steps.length;
  }
}