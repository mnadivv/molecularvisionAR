import 'package:equatable/equatable.dart';

abstract class ProgressEvent extends Equatable {
  const ProgressEvent();

  @override
  List<Object?> get props => [];
}

class ProgressFetchRequested extends ProgressEvent {
  const ProgressFetchRequested();
}

class ProgressRefreshRequested extends ProgressEvent {
  const ProgressRefreshRequested();
}
