import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/molecule/molecule_model.dart';
import '../../data/repositories/molecule_repository.dart';
import '../bloc/molecule_bloc.dart';
import '../bloc/molecule_event.dart';
import '../bloc/molecule_state.dart';
import '../widgets/molecule_3d_viewer.dart';
import '../widgets/molecule_info_card.dart';

/// Halaman Detail Visualisasi 3D Molekul dan Spesifikasi Teori Kimia (Figma node 130:2)
class MoleculeDetailScreen extends StatelessWidget {
  final MoleculeModel molecule;

  const MoleculeDetailScreen({
    super.key,
    required this.molecule,
  });

  @override
  Widget build(BuildContext context) {
    final view = _MoleculeDetailView(molecule: molecule);
    try {
      context.read<MoleculeBloc>();
      return view;
    } catch (_) {
      return BlocProvider(
        create: (context) => MoleculeBloc(
          repository: context.read<MoleculeRepository>(),
        )..add(const MoleculeFetchRequested()),
        child: view,
      );
    }
  }
}

class _MoleculeDetailView extends StatefulWidget {
  final MoleculeModel molecule;

  const _MoleculeDetailView({
    required this.molecule,
  });

  @override
  State<_MoleculeDetailView> createState() => _MoleculeDetailViewState();
}

class _MoleculeDetailViewState extends State<_MoleculeDetailView> {
  late MoleculeModel _currentMolecule;
  bool _isLoadingDetail = false;
  bool _isMarkedCompleted = false;

  @override
  void initState() {
    super.initState();
    _currentMolecule = widget.molecule;
    _fetchDetailIfNeeded();
  }

  Future<void> _fetchDetailIfNeeded() async {
    // Jika deskripsi belum lengkap ter-load dari list, ambil detail spesifik
    if (_currentMolecule.description == null ||
        _currentMolecule.description!.isEmpty) {
      setState(() => _isLoadingDetail = true);
      try {
        final repo = context.read<MoleculeRepository>();
        final detail = await repo.getMoleculeDetail(_currentMolecule.idMolecule);
        if (mounted) {
          setState(() {
            _currentMolecule = detail;
            _isLoadingDetail = false;
          });
        }
      } catch (_) {
        if (mounted) {
          setState(() => _isLoadingDetail = false);
        }
      }
    }
  }

  void _onMarkCompleted() {
    context.read<MoleculeBloc>().add(
          MoleculeProgressSubmitted(_currentMolecule.idMolecule),
        );
    setState(() {
      _isMarkedCompleted = true;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.check_circle_rounded, color: Colors.white),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                'Progres molekul ${_currentMolecule.name} berhasil disimpan!',
                style: const TextStyle(fontWeight: FontWeight.w500),
              ),
            ),
          ],
        ),
        backgroundColor: Colors.green.shade700,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        margin: const EdgeInsets.all(16),
        duration: const Duration(seconds: 2),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return BlocListener<MoleculeBloc, MoleculeState>(
      listener: (context, state) {
        if (state is MoleculeLoaded &&
            state.progressSuccessMoleculeId == _currentMolecule.idMolecule) {
          setState(() {
            _isMarkedCompleted = true;
          });
        }
      },
      child: Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(
          backgroundColor: Colors.transparent,
          elevation: 0,
          foregroundColor: Colors.white,
          leading: IconButton(
            icon: const Icon(Icons.arrow_back_ios_new_rounded),
            onPressed: () => Navigator.of(context).pop(),
          ),
          title: Text(
            'Molekul ${_currentMolecule.formula}',
            style: const TextStyle(
              color: Colors.white,
              fontSize: 19,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
        body: SafeArea(
          child: Column(
            children: [
              if (_isLoadingDetail)
                const LinearProgressIndicator(
                  color: AppColors.accent,
                  backgroundColor: Colors.transparent,
                  minHeight: 2,
                ),
              Expanded(
                child: SingleChildScrollView(
                  physics: const BouncingScrollPhysics(),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // 3D Interactive Viewer Card
                      Padding(
                        padding: const EdgeInsets.fromLTRB(20, 10, 20, 16),
                        child: Molecule3DViewer(
                          molecule: _currentMolecule,
                        ),
                      ),

                // Molecule Specification & Information Card
                BlocBuilder<MoleculeBloc, MoleculeState>(
                  builder: (context, state) {
                    final isCompletedFromState = (state is MoleculeLoaded) &&
                        state.completedMoleculeIds
                            .contains(_currentMolecule.idMolecule);
                    final isFinished = _isMarkedCompleted || isCompletedFromState;

                    return MoleculeInfoCard(
                      molecule: _currentMolecule,
                      isCompleted: isFinished,
                      onMarkCompleted: _onMarkCompleted,
                    );
                  },
                ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
