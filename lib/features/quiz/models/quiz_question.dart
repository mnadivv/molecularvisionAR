enum QuestionType {
  multipleChoice,
  multipleAnswer,
  trueFalse,
  matching,
  numericEssay,
}

enum StemAspect {
  science,
  technology,
  engineering,
  mathematics,
  integrated,
}

class MatchingPair {
  final String left;
  final String right;

  const MatchingPair({
    required this.left,
    required this.right,
  });
}

class QuizQuestion {
  final String id;
  final String question;
  final QuestionType type;
  final StemAspect stemAspect;

  final List<String> options;

  final String? correctAnswer;

  final List<String> correctAnswers;

  final double? numericAnswer;

  final double numericTolerance;

  final List<MatchingPair> matchingPairs;

  final String hint;

  final String explanation;

  const QuizQuestion({
    required this.id,
    required this.question,
    required this.type,
    required this.stemAspect,
    this.options = const [],
    this.correctAnswer,
    this.correctAnswers = const [],
    this.numericAnswer,
    this.numericTolerance = 0.0,
    this.matchingPairs = const [],
    this.hint = '',
    this.explanation = '',
  });
}