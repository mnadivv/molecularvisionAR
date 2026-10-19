import '../../../../core/constants/api_endpoints.dart';
import '../../../../core/network/dio_client.dart';
import '../../../../data/models/molecule/molecule_model.dart';

/// Repository untuk mengakses endpoint data katalog molekul kimia 3D dan progres belajar
class MoleculeRepository {
  final DioClient _dioClient;

  MoleculeRepository({DioClient? dioClient})
      : _dioClient = dioClient ?? DioClient();

  /// Mengambil daftar molekul kimia dengan opsi filter pencarian dan bentuk geometri
  Future<List<MoleculeModel>> getMolecules({
    String? search,
    String? shape,
  }) async {
    final queryParams = <String, dynamic>{};
    if (search != null && search.trim().isNotEmpty) {
      queryParams['search'] = search.trim();
    }
    if (shape != null && shape.trim().isNotEmpty && shape != 'Semua') {
      queryParams['shape'] = shape.trim();
    }

    final response = await _dioClient.get(
      ApiEndpoints.molecules,
      queryParameters: queryParams.isNotEmpty ? queryParams : null,
    );
    final data = response.data['data'] as List<dynamic>? ?? [];

    return data
        .whereType<Map<String, dynamic>>()
        .map(MoleculeModel.fromJson)
        .toList();
  }

  /// Mengambil detail lengkap suatu molekul kimia (termasuk deskripsi dan model 3D URL)
  Future<MoleculeModel> getMoleculeDetail(int id) async {
    final response = await _dioClient.get(ApiEndpoints.moleculeDetail(id));
    final data = response.data['data'] as Map<String, dynamic>;
    return MoleculeModel.fromJson(data);
  }

  /// Mengambil daftar ID molekul yang sudah diselesaikan siswa dari endpoint progress
  Future<Set<int>> getCompletedMoleculeIds() async {
    try {
      final response = await _dioClient.get(ApiEndpoints.progress);
      final data = response.data['data'] as Map<String, dynamic>? ?? {};
      final completedIds = data['completed_ids'] as Map<String, dynamic>? ?? {};
      final list = completedIds['molecules'] as List<dynamic>? ?? [];
      return list
          .map((e) => int.tryParse(e.toString()) ?? 0)
          .where((id) => id > 0)
          .toSet();
    } catch (_) {
      return {};
    }
  }

  /// Menyimpan progres eksplorasi molekul 3D siswa ke backend
  Future<bool> saveMoleculeProgress(
    int idMolecule, {
    bool isCompleted = true,
  }) async {
    final response = await _dioClient.post(
      ApiEndpoints.progressMolecule,
      data: {
        'id_molecule': idMolecule,
        'is_completed': isCompleted,
      },
    );
    return response.data['success'] == true;
  }
}
