import 'package:flutter/material.dart';

import '../../core/models/chemistry_topic.dart';
import '../../utils/app_colors.dart';

import '../ar/ar_menu_screen.dart';
import '../molecule/molecule_screen.dart';
import '../quiz/quiz_screen.dart';

class MateriDetailScreen extends StatelessWidget {
  final ChemistryTopic topic;

  const MateriDetailScreen({
    super.key,
    required this.topic,
  });

  String getDescription() {
    switch (topic.title) {

      case "Ikatan Kimia":
        return "Ikatan kimia merupakan gaya tarik-menarik antar atom yang menyebabkan atom-atom bergabung membentuk molekul atau senyawa yang stabil.";

      case "Termokimia":
        return "Termokimia mempelajari perubahan energi panas yang terjadi selama reaksi kimia berlangsung.";

      case "Kalorimetri":
        return "Kalorimetri mempelajari cara mengukur jumlah kalor yang dilepas atau diserap suatu reaksi.";

case "Laju Reaksi":
  return """
Apa itu Laju Reaksi?

Laju reaksi adalah besarnya kecepatan berlangsungnya suatu reaksi kimia. Laju reaksi dapat dinyatakan sebagai laju berkurangnya pereaksi (reaktan) atau bertambahnya produk per satuan waktu.

Reaksi:
p A(g) + q B(g) → r C(g) + s D(g)

Dengan:
- pA(g) + qB(g) = Reaktan
- rC(g) + sD(g) = Produk
- p, q, r, s = Koefisien reaksi

Laju rata-rata:

VA = -d[A]/dt
(Pengurangan konsentrasi A per satuan waktu)

VB = -d[B]/dt
(Pengurangan konsentrasi B per satuan waktu)

VC = d[C]/dt
(Penambahan konsentrasi C per satuan waktu)

VD = d[D]/dt
(Penambahan konsentrasi D per satuan waktu)

Tanda negatif (-) menunjukkan pengurangan konsentrasi, sedangkan tanda positif menunjukkan penambahan konsentrasi.

Setiap reaksi kimia memiliki nilai laju reaksi yang berbeda sehingga dapat ditentukan menggunakan persamaan berikut:

Persamaan Laju Reaksi:
v = k[A]^x[B]^y

Keterangan:
- k = Tetapan laju reaksi
- x = Orde reaksi terhadap pereaksi A
- y = Orde reaksi terhadap pereaksi B
- x + y = Orde total reaksi

Orde Reaksi

Orde reaksi menunjukkan pangkat konsentrasi dalam persamaan laju. Nilainya ditentukan melalui eksperimen dan bukan berdasarkan koefisien reaksi.

1. Reaksi Orde 0
Laju reaksi tidak dipengaruhi oleh perubahan konsentrasi pereaksi. Nilai laju reaksi selalu konstan dan sama dengan tetapan laju (k).

2. Reaksi Orde 1
Jika konsentrasi pereaksi dinaikkan dua kali, maka laju reaksi juga meningkat dua kali. Secara umum, jika konsentrasi dikalikan n, maka laju reaksi menjadi n kali lebih besar.

3. Reaksi Orde 2
Jika konsentrasi pereaksi dinaikkan dua kali, maka laju reaksi meningkat empat kali. Secara umum, jika konsentrasi dikalikan n, maka laju reaksi menjadi n² kali lebih besar.

Tetapan Laju Reaksi (k)

Tetapan laju reaksi (k) adalah konstanta yang dipengaruhi oleh jenis pereaksi, suhu, dan katalis. Reaksi yang berlangsung lebih cepat memiliki nilai k yang lebih besar dibandingkan reaksi yang berlangsung lambat.

Faktor-Faktor yang Mempengaruhi Laju Reaksi

1. Luas Permukaan
Semakin luas permukaan zat pereaksi, semakin cepat laju reaksinya karena jumlah tumbukan antarmolekul meningkat.

2. Konsentrasi
Semakin tinggi konsentrasi reaktan, semakin banyak partikel yang bertumbukan sehingga laju reaksi meningkat.

3. Suhu
Semakin tinggi suhu, energi kinetik partikel meningkat sehingga frekuensi dan energi tumbukan bertambah.

4. Katalis
Katalis adalah zat yang mempercepat reaksi tanpa ikut habis bereaksi. Katalis menyediakan jalur reaksi alternatif dengan energi aktivasi yang lebih rendah sehingga reaksi berlangsung lebih cepat.
""";

      case "Asam Basa":
        return "Asam basa mempelajari sifat larutan berdasarkan teori Arrhenius, Bronsted-Lowry, dan Lewis.";

      case "Hidrolisis":
        return "Hidrolisis merupakan reaksi ion garam dengan air yang menghasilkan sifat asam ataupun basa.";

      case "Hasil Kali Kelarutan":
        return "Konsep Ksp digunakan untuk menentukan kelarutan suatu senyawa ionik.";

      case "Kesetimbangan Kimia":
        return "Kesetimbangan kimia terjadi ketika laju reaksi maju sama dengan laju reaksi balik.";

      case "Sel Volta":
        return "Sel Volta menghasilkan energi listrik melalui reaksi redoks spontan.";

      case "Elektrolisis":
        return "Elektrolisis menggunakan energi listrik untuk menjalankan reaksi redoks yang tidak spontan.";

      case "Hidrokarbon":
        return "Hidrokarbon merupakan senyawa yang hanya tersusun dari atom karbon dan hidrogen.";

      case "Daur Ulang Polimer":
        return "Daur ulang polimer bertujuan mengurangi limbah plastik dengan memanfaatkan kembali material polimer.";

      default:
        return "Materi Kimia.";
    }
  }
    @override
  Widget build(BuildContext context) {

    return Scaffold(

      backgroundColor: AppColors.background,

      appBar: AppBar(

        backgroundColor: Colors.transparent,

        elevation: 0,

        iconTheme: const IconThemeData(
          color: Colors.white,
        ),

        title: Text(
          topic.title,
          style: const TextStyle(
            color: Colors.white,
          ),
        ),

      ),

      body: ListView(

        padding: const EdgeInsets.all(22),

        children: [

          CircleAvatar(

            radius: 45,

            backgroundColor:
                topic.color.withValues(alpha: .18),

            child: Icon(
              topic.icon,
              size: 45,
              color: topic.color,
            ),

          ),

          const SizedBox(height: 25),

          Text(

            topic.title,

            style: const TextStyle(

              color: Colors.white,

              fontSize: 28,

              fontWeight: FontWeight.bold,

            ),

          ),

          const SizedBox(height: 18),

          Text(

            getDescription(),

            style: TextStyle(

              color: Colors.white.withValues(alpha: .75),

              fontSize: 16,

              height: 1.7,

            ),

          ),

          const SizedBox(height: 35),
          const Text(

            "Menu Pembelajaran",

            style: TextStyle(

              color: Colors.white,

              fontSize: 20,

              fontWeight: FontWeight.bold,

            ),

          ),

          const SizedBox(height: 18),

          ElevatedButton.icon(

            icon: const Icon(Icons.science),

            label: const Text("Lihat Detail Molekul"),

            style: ElevatedButton.styleFrom(

              minimumSize: const Size.fromHeight(55),

            ),

            onPressed: () {

              Navigator.push(

                context,

                MaterialPageRoute(

                  builder: (_) => const MoleculeScreen(),

                ),

              );

            },

          ),

          const SizedBox(height: 15),

          ElevatedButton.icon(

            icon: const Icon(Icons.view_in_ar),

            label: const Text("Mulai Augmented Reality"),

            style: ElevatedButton.styleFrom(

              minimumSize: const Size.fromHeight(55),

            ),

            onPressed: () {

              Navigator.push(

                context,

                MaterialPageRoute(

                  builder: (_) => const ARMenuScreen(),

                ),

              );

            },

          ),

          const SizedBox(height: 15),

ElevatedButton.icon(
  icon: const Icon(Icons.quiz),
  label: const Text("Kerjakan Quiz"),
  style: ElevatedButton.styleFrom(
    minimumSize: const Size.fromHeight(55),
  ),
  onPressed: () {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => QuizScreen(
          materialId: topic.title,
          materialName: topic.title,
        ),
      ),
    );
  },
),

          const SizedBox(height: 30),

        ],

      ),

    );

  }

}