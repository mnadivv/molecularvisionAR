import 'package:equatable/equatable.dart';
import 'package:molecular_vision_ar/data/models/progress/progress_model.dart';

abstract class ProgressState extends Equatable {
  const ProgressState();

  @override
  List<Object?> get props => [];
}

class ProgressInitial extends ProgressState {
  const ProgressInitial();
}

class ProgressLoading extends ProgressState {
  const ProgressLoading();
}

class ProgressLoaded extends ProgressState {
  final ProgressModel progress;
  final bool isRefreshing;

  const ProgressLoaded({
    required this.progress,
    this.isRefreshing = false,
  });

  ProgressLoaded copyWith({
    ProgressModel? progress,
    bool? isRefreshing,
  }) {
    return ProgressLoaded(
      progress: progress ?? this.progress,
      isRefreshing: isRefreshing ?? this.isRefreshing,
    );
  }

  @override
  List<Object?> get props => [progress, isRefreshing];
}

class ProgressFailure extends ProgressState {
  final String message;

  const ProgressFailure(this.message);

  @override
  List<Object?> get props => [message];
}
