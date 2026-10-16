import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/data/models/auth/auth_response_model.dart';
import 'package:molecular_vision_ar/data/models/auth/degree_model.dart';
import 'package:molecular_vision_ar/data/models/auth/role_model.dart';
import 'package:molecular_vision_ar/data/models/auth/user_model.dart';
import 'package:molecular_vision_ar/data/models/category/category_model.dart';
import 'package:molecular_vision_ar/data/models/exercise/exercise_model.dart';
import 'package:molecular_vision_ar/data/models/material/material_model.dart';
import 'package:molecular_vision_ar/data/models/molecule/molecule_model.dart';
import 'package:molecular_vision_ar/data/models/progress/progress_model.dart';

void main() {
  group('Data Models JSON Deserialization & Serialization', () {
    test('RoleModel and DegreeModel parse correctly', () {
      final role = RoleModel.fromJson({'id': 3, 'name': 'siswa (mobile)'});
      expect(role.id, equals(3));
      expect(role.name, equals('siswa (mobile)'));
      expect(role.toJson()['id'], equals(3));

      final degree = DegreeModel.fromJson({'id_degree': 1, 'name': 'Kelas 10'});
      expect(degree.idDegree, equals(1));
      expect(degree.name, equals('Kelas 10'));
      expect(degree.toJson()['id_degree'], equals(1));
    });

    test('UserModel and AuthResponseModel parse correctly', () {
      final json = {
        'id_user': 15,
        'name': 'Budi Santoso',
        'email': 'budi@example.com',
        'school': 'SMAN 1 Jakarta',
        'id_degree': 1,
        'id_role': 3,
        'role': {'id': 3, 'name': 'siswa (mobile)'},
        'degree': {'id_degree': 1, 'name': 'Kelas 10'},
      };

      final user = UserModel.fromJson(json);
      expect(user.idUser, equals(15));
      expect(user.name, equals('Budi Santoso'));
      expect(user.role?.name, equals('siswa (mobile)'));
      expect(user.degree?.name, equals('Kelas 10'));

      final authResponse = AuthResponseModel.fromJson({
        'user': json,
        'token': 'test-sanctum-token',
        'token_type': 'Bearer',
      });
      expect(authResponse.token, equals('test-sanctum-token'));
      expect(authResponse.user.email, equals('budi@example.com'));
    });

    test('CategoryModel parses correctly', () {
      final category = CategoryModel.fromJson({
        'id_category': 1,
        'name': 'Stoikiometri',
        'order': 1,
        'description': 'Mempelajari konsep mol.',
        'materials_count': 4,
        'questions_count': 15,
      });

      expect(category.idCategory, equals(1));
      expect(category.materialsCount, equals(4));
      expect(category.questionsCount, equals(15));
    });

    test('MoleculeModel parses correctly', () {
      final molecule = MoleculeModel.fromJson({
        'id_molecule': 1,
        'name': 'Air',
        'order': 1,
        'formula': 'H2O',
        'shape': 'Bent',
        'bent': '104.5°',
        'bond_type': 'Kovalen Polar',
        'description': 'Molekul air',
        'model_3d_url': 'https://example.com/h2o.glb',
      });

      expect(molecule.idMolecule, equals(1));
      expect(molecule.formula, equals('H2O'));
      expect(molecule.model3dUrl, equals('https://example.com/h2o.glb'));
    });

    test('MaterialModel parses correctly with nested CategoryModel', () {
      final material = MaterialModel.fromJson({
        'id_material': 1,
        'id_category': 1,
        'name': 'Konsep Mol',
        'order': 1,
        'description': 'Pengantar mol',
        'content': '<h2>Mol</h2>',
        'category': {
          'id_category': 1,
          'name': 'Stoikiometri',
        },
      });

      expect(material.idMaterial, equals(1));
      expect(material.category?.name, equals('Stoikiometri'));
    });

    test('ProgressModel parses correctly', () {
      final progress = ProgressModel.fromJson({
        'materials': {
          'total': 12,
          'completed': 8,
          'uncompleted': 4,
          'percentage': 66.7,
        },
        'molecules': {
          'total': 10,
          'completed': 5,
          'uncompleted': 5,
          'percentage': 50.0,
        },
        'quizzes': {
          'total': 4,
          'completed': 2,
          'uncompleted': 2,
          'percentage': 50.0,
        },
        'completed_material_ids': [1, 2, 3],
        'completed_molecule_ids': [1, 2],
        'completed_category_ids': [1],
      });

      expect(progress.materials.total, equals(12));
      expect(progress.materials.percentage, equals(66.7));
      expect(progress.completedMaterialIds, equals([1, 2, 3]));
    });

    test('Exercise Models parse correctly', () {
      final question = ExerciseQuestionModel.fromJson({
        'id_question': 10,
        'id_category': 1,
        'question_text': 'Berapa sudut ikatan air?',
        'question_type': 'multiple_choice',
        'points': 10,
        'options': [
          {'id_option': 1, 'option_text': '104.5°'},
          {'id_option': 2, 'option_text': '180°'},
        ],
      });

      expect(question.idQuestion, equals(10));
      expect(question.options.length, equals(2));
      expect(question.options.first.optionText, equals('104.5°'));

      final result = ExerciseResultModel.fromJson({
        'total_questions': 10,
        'correct_answers': 8,
        'score': 80.0,
        'passed': true,
      });

      expect(result.score, equals(80.0));
      expect(result.passed, isTrue);
    });
  });
}
