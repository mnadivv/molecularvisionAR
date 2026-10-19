import 'package:equatable/equatable.dart';
import '../../../../data/models/molecule/molecule_model.dart';

abstract class MoleculeState extends Equatable {
  const MoleculeState();

  @override
  List<Object?> get props => [];
}

class MoleculeInitial extends MoleculeState {
  const MoleculeInitial();
}

class MoleculeLoading extends MoleculeState {
  const MoleculeLoading();
}

class MoleculeLoaded extends MoleculeState {
  final List<MoleculeModel> molecules;
  final List<String> availableShapes;
  final String? selectedShape;
  final String searchQuery;
  final bool isFiltering;
  final int? progressSuccessMoleculeId;
  final Set<int> completedMoleculeIds;

  const MoleculeLoaded({
    required this.molecules,
    required this.availableShapes,
    this.selectedShape,
    this.searchQuery = '',
    this.isFiltering = false,
    this.progressSuccessMoleculeId,
    this.completedMoleculeIds = const {},
  });

  MoleculeLoaded copyWith({
    List<MoleculeModel>? molecules,
    List<String>? availableShapes,
    String? selectedShape,
    bool clearSelectedShape = false,
    String? searchQuery,
    bool? isFiltering,
    int? progressSuccessMoleculeId,
    Set<int>? completedMoleculeIds,
  }) {
    return MoleculeLoaded(
      molecules: molecules ?? this.molecules,
      availableShapes: availableShapes ?? this.availableShapes,
      selectedShape: clearSelectedShape
          ? null
          : (selectedShape ?? this.selectedShape),
      searchQuery: searchQuery ?? this.searchQuery,
      isFiltering: isFiltering ?? this.isFiltering,
      progressSuccessMoleculeId: progressSuccessMoleculeId,
      completedMoleculeIds: completedMoleculeIds ?? this.completedMoleculeIds,
    );
  }

  @override
  List<Object?> get props => [
        molecules,
        availableShapes,
        selectedShape,
        searchQuery,
        isFiltering,
        progressSuccessMoleculeId,
        completedMoleculeIds,
      ];
}

class MoleculeFailure extends MoleculeState {
  final String message;

  const MoleculeFailure(this.message);

  @override
  List<Object?> get props => [message];
}
