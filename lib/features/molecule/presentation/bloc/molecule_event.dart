import 'package:equatable/equatable.dart';

abstract class MoleculeEvent extends Equatable {
  const MoleculeEvent();

  @override
  List<Object?> get props => [];
}

/// Memicu pengambilan data awal katalog molekul 3D
class MoleculeFetchRequested extends MoleculeEvent {
  final String? shape;
  final String? search;

  const MoleculeFetchRequested({this.shape, this.search});

  @override
  List<Object?> get props => [shape, search];
}

/// Memfilter molekul berdasarkan bentuk geometri (null / 'Semua' = seluruh molekul)
class MoleculeShapeSelected extends MoleculeEvent {
  final String? shape;

  const MoleculeShapeSelected(this.shape);

  @override
  List<Object?> get props => [shape];
}

/// Menjalankan pencarian molekul berdasarkan rumus atau nama kimia
class MoleculeSearchSubmitted extends MoleculeEvent {
  final String query;

  const MoleculeSearchSubmitted(this.query);

  @override
  List<Object?> get props => [query];
}

/// Memicu refresh data dari pull-to-refresh
class MoleculeRefreshRequested extends MoleculeEvent {
  const MoleculeRefreshRequested();
}

/// Merekam progres eksplorasi molekul 3D siswa
class MoleculeProgressSubmitted extends MoleculeEvent {
  final int idMolecule;

  const MoleculeProgressSubmitted(this.idMolecule);

  @override
  List<Object?> get props => [idMolecule];
}
