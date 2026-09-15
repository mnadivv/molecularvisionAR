import '../models/quiz_question.dart';

class QuizData {
  static const List<QuizQuestion> lajuReaksiQuestions = [
    QuizQuestion(
      id: 'lr_001',
      type: QuestionType.multipleChoice,
      stemAspect: StemAspect.science,
      question:
          'Dalam suatu percobaan, serbuk kalsium karbonat bereaksi lebih cepat dengan larutan asam dibandingkan bongkahan kalsium karbonat dengan massa yang sama. Faktor yang paling tepat menjelaskan fenomena tersebut adalah ...',
      options: [
        'Suhu',
        'Konsentrasi',
        'Luas permukaan',
        'Tekanan',
        'Warna zat',
      ],
      correctAnswer: 'Luas permukaan',
      hint:
          'Bandingkan luas bidang kontak antara zat padat dengan larutan.',
      explanation:
          'Serbuk memiliki luas permukaan yang lebih besar daripada bongkahan dengan massa yang sama. Luas permukaan yang lebih besar menyebabkan lebih banyak partikel dapat mengalami tumbukan dengan pereaksi sehingga frekuensi tumbukan efektif meningkat dan reaksi berlangsung lebih cepat.',
    ),

    QuizQuestion(
      id: 'lr_002',
      type: QuestionType.multipleChoice,
      stemAspect: StemAspect.science,
      question:
          'Menurut teori tumbukan, reaksi kimia dapat berlangsung apabila partikel pereaksi ...',
      options: [
        'Selalu bergerak dengan kecepatan yang sama',
        'Bertumbukan dengan energi dan orientasi yang sesuai',
        'Memiliki massa yang sama',
        'Tidak mengalami tumbukan',
        'Berada pada suhu 0°C',
      ],
      correctAnswer:
          'Bertumbukan dengan energi dan orientasi yang sesuai',
      hint:
          'Tidak semua tumbukan menghasilkan reaksi.',
      explanation:
          'Menurut teori tumbukan, hanya tumbukan efektif yang dapat menghasilkan reaksi. Tumbukan tersebut harus memiliki energi yang cukup untuk melewati energi aktivasi dan orientasi yang sesuai.',
    ),

    QuizQuestion(
      id: 'lr_003',
      type: QuestionType.multipleChoice,
      stemAspect: StemAspect.technology,
      question:
          'Seorang siswa menggunakan aplikasi simulasi untuk membandingkan reaksi pada suhu 25°C dan 40°C. Pada suhu 40°C, jumlah tumbukan efektif yang diamati lebih banyak. Kesimpulan yang paling tepat adalah ...',
      options: [
        'Kenaikan suhu menurunkan energi kinetik partikel',
        'Kenaikan suhu meningkatkan energi kinetik partikel',
        'Kenaikan suhu menghilangkan partikel pereaksi',
        'Kenaikan suhu menghentikan tumbukan',
        'Suhu tidak berhubungan dengan laju reaksi',
      ],
      correctAnswer:
          'Kenaikan suhu meningkatkan energi kinetik partikel',
      hint:
          'Hubungkan suhu dengan energi kinetik partikel.',
      explanation:
          'Peningkatan suhu menyebabkan energi kinetik rata-rata partikel meningkat. Akibatnya, tumbukan menjadi lebih sering dan lebih banyak partikel memiliki energi yang cukup untuk menghasilkan tumbukan efektif.',
    ),

    QuizQuestion(
      id: 'lr_004',
      type: QuestionType.multipleChoice,
      stemAspect: StemAspect.engineering,
      question:
          'Sebuah industri ingin mempercepat suatu reaksi tanpa mengubah hasil akhir reaksi. Pilihan yang paling sesuai adalah ...',
      options: [
        'Menggunakan katalis yang sesuai',
        'Mengurangi konsentrasi pereaksi',
        'Mengurangi luas permukaan',
        'Menghilangkan semua tumbukan',
        'Menurunkan suhu secara drastis',
      ],
      correctAnswer:
          'Menggunakan katalis yang sesuai',
      hint:
          'Cari cara mempercepat reaksi dengan jalur reaksi alternatif.',
      explanation:
          'Katalis menyediakan jalur reaksi alternatif dengan energi aktivasi yang lebih rendah. Katalis mempercepat tercapainya reaksi tanpa menjadi bagian permanen dari produk akhir.',
    ),

    QuizQuestion(
      id: 'lr_005',
      type: QuestionType.multipleAnswer,
      stemAspect: StemAspect.integrated,
      question:
          'Pilih SEMUA faktor yang dapat meningkatkan laju reaksi dalam kondisi yang sesuai.',
      options: [
        'Meningkatkan suhu',
        'Meningkatkan konsentrasi pereaksi',
        'Memperbesar luas permukaan zat padat',
        'Menggunakan katalis',
        'Mengurangi tumbukan efektif',
      ],
      correctAnswers: [
        'Meningkatkan suhu',
        'Meningkatkan konsentrasi pereaksi',
        'Memperbesar luas permukaan zat padat',
        'Menggunakan katalis',
      ],
      hint:
          'Pilih faktor yang meningkatkan frekuensi tumbukan efektif atau menurunkan energi aktivasi.',
      explanation:
          'Peningkatan suhu meningkatkan energi kinetik partikel. Peningkatan konsentrasi meningkatkan frekuensi tumbukan. Luas permukaan yang lebih besar meningkatkan bidang kontak. Katalis menyediakan jalur reaksi dengan energi aktivasi lebih rendah.',
    ),

    QuizQuestion(
      id: 'lr_006',
      type: QuestionType.multipleAnswer,
      stemAspect: StemAspect.science,
      question:
          'Pilih SEMUA pernyataan yang sesuai dengan teori tumbukan.',
      options: [
        'Tidak semua tumbukan menghasilkan reaksi',
        'Tumbukan harus memiliki energi yang cukup',
        'Orientasi partikel dapat menentukan keberhasilan tumbukan',
        'Tumbukan efektif dapat menghasilkan produk',
        'Semua tumbukan pasti menghasilkan reaksi',
      ],
      correctAnswers: [
        'Tidak semua tumbukan menghasilkan reaksi',
        'Tumbukan harus memiliki energi yang cukup',
        'Orientasi partikel dapat menentukan keberhasilan tumbukan',
        'Tumbukan efektif dapat menghasilkan produk',
      ],
      hint:
          'Perhatikan syarat terjadinya tumbukan efektif.',
      explanation:
          'Tumbukan efektif membutuhkan energi yang cukup untuk melewati energi aktivasi dan orientasi yang sesuai. Karena itu, tidak semua tumbukan menghasilkan reaksi.',
    ),

    QuizQuestion(
      id: 'lr_007',
      type: QuestionType.trueFalse,
      stemAspect: StemAspect.science,
      question:
          'Meningkatkan suhu dapat meningkatkan laju reaksi karena energi kinetik rata-rata partikel meningkat dan peluang tumbukan efektif menjadi lebih besar.',
      options: [
        'Benar',
        'Salah',
      ],
      correctAnswer: 'Benar',
      hint:
          'Hubungkan suhu dengan energi kinetik dan tumbukan efektif.',
      explanation:
          'Pernyataan tersebut benar. Ketika suhu meningkat, energi kinetik rata-rata partikel meningkat sehingga lebih banyak partikel dapat memiliki energi yang memenuhi energi aktivasi.',
    ),

    QuizQuestion(
      id: 'lr_008',
      type: QuestionType.matching,
      stemAspect: StemAspect.engineering,
      question:
          'Jodohkan faktor dengan mekanisme pengaruhnya terhadap laju reaksi.',
      matchingPairs: [
        MatchingPair(
          left: 'Suhu dinaikkan',
          right: 'Energi kinetik partikel meningkat',
        ),
        MatchingPair(
          left: 'Konsentrasi dinaikkan',
          right: 'Frekuensi tumbukan meningkat',
        ),
        MatchingPair(
          left: 'Luas permukaan diperbesar',
          right: 'Bidang kontak bertambah',
        ),
        MatchingPair(
          left: 'Katalis ditambahkan',
          right: 'Energi aktivasi diturunkan',
        ),
      ],
      hint:
          'Cari hubungan langsung antara faktor dan mekanisme tumbukan.',
      explanation:
          'Setiap faktor memengaruhi laju reaksi melalui mekanisme yang berbeda. Suhu meningkatkan energi kinetik, konsentrasi meningkatkan frekuensi tumbukan, luas permukaan memperbesar bidang kontak, sedangkan katalis menurunkan energi aktivasi melalui jalur alternatif.',
    ),

    QuizQuestion(
      id: 'lr_009',
      type: QuestionType.numericEssay,
      stemAspect: StemAspect.mathematics,
      question:
          'Suatu reaksi menghasilkan 25 mL gas dalam waktu 5 sekon. Berapakah laju pembentukan gas tersebut dalam mL/s? Masukkan ANGKA SAJA, tanpa satuan.',
      numericAnswer: 5,
      numericTolerance: 0.0,
      hint:
          'Gunakan rumus laju = perubahan jumlah zat / waktu.',
      explanation:
          'Laju pembentukan gas dihitung dengan membagi volume gas yang terbentuk dengan waktu. 25 dibagi 5 menghasilkan 5. Jadi jawaban angka yang benar adalah 5.',
    ),

    QuizQuestion(
      id: 'lr_010',
      type: QuestionType.numericEssay,
      stemAspect: StemAspect.mathematics,
      question:
          'Konsentrasi suatu pereaksi berkurang dari 0,80 M menjadi 0,20 M dalam waktu 30 sekon. Berapakah besar laju rata-rata berkurangnya konsentrasi dalam M/s? Masukkan ANGKA SAJA, tanpa satuan.',
      numericAnswer: 0.02,
      numericTolerance: 0.0001,
      hint:
          'Hitung perubahan konsentrasi terlebih dahulu, kemudian bagi dengan waktu.',
      explanation:
          'Perubahan konsentrasi adalah 0,80 - 0,20 = 0,60 M. Kemudian 0,60 dibagi 30 sekon = 0,02. Jadi jawaban angka yang benar adalah 0,02.',
    ),
  ];

  static List<QuizQuestion> getQuestionsForMaterial(
    String materialId,
  ) {
    switch (materialId) {
      case 'laju_reaksi':
        return lajuReaksiQuestions;

      default:
        return [];
    }
  }
}