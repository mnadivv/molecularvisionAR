import 'package:equatable/equatable.dart';

abstract class MateriEvent extends Equatable {
  const MateriEvent();

  @override
  List<Object?> get props => [];
}

/// Memicu pengambilan data awal seluruh kategori dan materi pembelajaran
class MateriFetchRequested extends MateriEvent {
  final int? idCategory;
  final String? search;

  const MateriFetchRequested({this.idCategory, this.search});

  @override
  List<Object?> get props => [idCategory, search];
}

/// Mengubah filter kategori yang sedang dipilih (null = Semua Kategori)
class MateriCategorySelected extends MateriEvent {
  final int? idCategory;

  const MateriCategorySelected(this.idCategory);

  @override
  List<Object?> get props => [idCategory];
}

/// Menjalankan pencarian materi berdasarkan teks input
class MateriSearchSubmitted extends MateriEvent {
  final String query;

  const MateriSearchSubmitted(this.query);

  @override
  List<Object?> get props => [query];
}

/// Memicu refresh data dari drag-to-refresh
class MateriRefreshRequested extends MateriEvent {
  const MateriRefreshRequested();
}

/// Menyimpan status selesai membaca materi pembelajaran ke backend
class MateriProgressSubmitted extends MateriEvent {
  final int idMaterial;

  const MateriProgressSubmitted(this.idMaterial);

  @override
  List<Object?> get props => [idMaterial];
}
