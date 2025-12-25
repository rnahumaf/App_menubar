import Cocoa

final class ClipboardController {
    private let pasteboard = NSPasteboard.general
    private var timer: Timer?

    private var lastChangeCount: Int
    private var ignoreChangeCount: Int?

    private let store: StatsStore

    private(set) var sessionAffected: Int64 = 0
    private(set) var allTimeAffected: Int64

    var enabled: Bool = true

    var onCountersChanged: (() -> Void)?

    init(store: StatsStore) {
        self.store = store
        self.allTimeAffected = store.allTimeAffectedChars
        self.lastChangeCount = pasteboard.changeCount
    }

    func start(pollIntervalSeconds: TimeInterval = 0.40) {
        stop()
        timer = Timer.scheduledTimer(withTimeInterval: pollIntervalSeconds, repeats: true) { [weak self] _ in
            self?.tick()
        }
        RunLoop.main.add(timer!, forMode: .common)
    }

    func stop() {
        timer?.invalidate()
        timer = nil
    }

    private func tick() {
        let cc = pasteboard.changeCount
        if cc == lastChangeCount { return }
        lastChangeCount = cc

        if let ignore = ignoreChangeCount, cc == ignore {
            return
        }

        if !enabled { return }

        guard let original = pasteboard.string(forType: .string) else {
            return
        }

        let result = ClipboardNormalizer.normalizeWithStats(original)
        if result.totalAffected == 0 { return }

        // escreve de volta (anti-loop via ignoreChangeCount)
        pasteboard.clearContents()
        let ok = pasteboard.setString(result.text, forType: .string)
        if ok {
            let newCC = pasteboard.changeCount
            ignoreChangeCount = newCC
            lastChangeCount = newCC
        }

        sessionAffected += Int64(result.totalAffected)
        allTimeAffected += Int64(result.totalAffected)

        store.allTimeAffectedChars = allTimeAffected
        store.save()

        onCountersChanged?()
    }
}
