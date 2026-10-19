import 'package:flutter/material.dart';

/// Search bar widget persis sesuai Figma node 65:127 - 65:130
class MateriSearchBar extends StatefulWidget {
  final ValueChanged<String> onSearch;
  final VoidCallback onClear;
  final String initialValue;

  const MateriSearchBar({
    super.key,
    required this.onSearch,
    required this.onClear,
    this.initialValue = '',
  });

  @override
  State<MateriSearchBar> createState() => _MateriSearchBarState();
}

class _MateriSearchBarState extends State<MateriSearchBar> {
  late final TextEditingController _controller;
  bool _hasText = false;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialValue);
    _hasText = widget.initialValue.isNotEmpty;

    _controller.addListener(() {
      final hasText = _controller.text.trim().isNotEmpty;
      if (hasText != _hasText) {
        setState(() {
          _hasText = hasText;
        });
      }
    });
  }

  @override
  void didUpdateWidget(covariant MateriSearchBar oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.initialValue != widget.initialValue &&
        widget.initialValue != _controller.text) {
      _controller.text = widget.initialValue;
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _triggerSearch() {
    FocusScope.of(context).unfocus();
    widget.onSearch(_controller.text.trim());
  }

  void _triggerClear() {
    _controller.clear();
    FocusScope.of(context).unfocus();
    widget.onClear();
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        // Text Input
        Expanded(
          child: Container(
            height: 48,
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.15),
                  blurRadius: 10,
                  offset: const Offset(0, 4),
                ),
              ],
            ),
            child: TextField(
              controller: _controller,
              textInputAction: TextInputAction.search,
              onSubmitted: (_) => _triggerSearch(),
              style: const TextStyle(
                color: Color(0xFF1E293B),
                fontSize: 14,
                fontWeight: FontWeight.w500,
              ),
              decoration: InputDecoration(
                hintText: 'Cari materi...',
                hintStyle: TextStyle(
                  color: Colors.grey.shade500,
                  fontSize: 14,
                ),
                prefixIcon: Icon(
                  Icons.search_rounded,
                  color: Colors.grey.shade600,
                  size: 20,
                ),
                suffixIcon: _hasText
                    ? IconButton(
                        icon: const Icon(Icons.close_rounded, size: 18),
                        color: Colors.grey.shade600,
                        splashRadius: 18,
                        onPressed: _triggerClear,
                      )
                    : null,
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 14,
                  vertical: 12,
                ),
                border: InputBorder.none,
                enabledBorder: InputBorder.none,
                focusedBorder: InputBorder.none,
              ),
            ),
          ),
        ),
        const SizedBox(width: 10),
        // Tombol Cari
        SizedBox(
          height: 48,
          child: ElevatedButton(
            onPressed: _triggerSearch,
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.white,
              foregroundColor: const Color(0xFF0F172A),
              elevation: 4,
              shadowColor: Colors.black.withValues(alpha: 0.2),
              padding: const EdgeInsets.symmetric(horizontal: 20),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
            ),
            child: const Text(
              'Cari',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.bold,
                letterSpacing: 0.3,
              ),
            ),
          ),
        ),
      ],
    );
  }
}
