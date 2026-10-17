import '../../../../core/constants/api_endpoints.dart';
import '../../../../core/network/dio_client.dart';
import '../../../../data/models/category/category_model.dart';
import '../../../../data/models/material/material_model.dart';

/// Repository untuk mengakses endpoint data kategori dan materi pembelajaran kimia
class MateriRepository {
  final DioClient _dioClient;

  MateriRepository({DioClient? dioClient})
      : _dioClient = dioClient ?? DioClient();

  /// Mengambil seluruh daftar kategori materi kimia dari backend
  Future<List<CategoryModel>> getCategories() async {
    final response = await _dioClient.get(ApiEndpoints.categories);
    final data = response.data['data'] as List<dynamic>? ?? [];

    return data
        .whereType<Map<String, dynamic>>()
        .map(CategoryModel.fromJson)
        .toList();
  }

  /// Mengambil daftar materi kimia dengan opsi filter kategori dan pencarian judul
  Future<List<MaterialModel>> getMaterials({
    int? idCategory,
    String? search,
  }) async {
    final queryParams = <String, dynamic>{};
    if (idCategory != null) {
      queryParams['id_category'] = idCategory;
    }
    if (search != null && search.trim().isNotEmpty) {
      queryParams['search'] = search.trim();
    }

    final response = await _dioClient.get(
      ApiEndpoints.materials,
      queryParameters: queryParams.isNotEmpty ? queryParams : null,
    );
    final data = response.data['data'] as List<dynamic>? ?? [];

    return data
        .whereType<Map<String, dynamic>>()
        .map(MaterialModel.fromJson)
        .toList();
  }

  /// Mengambil detail lengkap satu materi (termasuk isi materi/konten teori)
  Future<MaterialModel> getMaterialDetail(int id) async {
    final response = await _dioClient.get(ApiEndpoints.materialDetail(id));
    final data = response.data['data'] as Map<String, dynamic>;
    return MaterialModel.fromJson(data);
  }
}
