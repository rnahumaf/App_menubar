import Cocoa

final class AppDelegate: NSObject, NSApplicationDelegate {
    private var statusItem: NSStatusItem!

    private let sessionItem = NSMenuItem(title: "Session affected chars: 0", action: nil, keyEquivalent: "")
    private let allTimeItem = NSMenuItem(title: "All time affected chars: 0", action: nil, keyEquivalent: "")

    private let enabledItem = NSMenuItem(title: "Enabled", action: #selector(toggleEnabled(_:)), keyEquivalent: "")
    private let quitItem = NSMenuItem(title: "Quit", action: #selector(quit(_:)), keyEquivalent: "q")

    private let store = StatsStore.load()
    private var controller: ClipboardController!

    func applicationDidFinishLaunching(_ notification: Notification) {
        NSApp.setActivationPolicy(.accessory)

        controller = ClipboardController(store: store)
        controller.onCountersChanged = { [weak self] in
            self?.updateMenuCounters()
        }

        sessionItem.isEnabled = false
        allTimeItem.isEnabled = false

        enabledItem.target = self
        enabledItem.state = .on

        quitItem.target = self

        let menu = NSMenu()
        menu.addItem(sessionItem)
        menu.addItem(allTimeItem)
        menu.addItem(NSMenuItem.separator())
        menu.addItem(enabledItem)
        menu.addItem(NSMenuItem.separator())
        menu.addItem(quitItem)

        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        if let button = statusItem.button {
            button.image = NSImage(systemSymbolName: "doc.on.clipboard", accessibilityDescription: "Tiny Clipboard Normalizer")
            button.image?.isTemplate = true
        }
        statusItem.menu = menu

        updateMenuCounters()
        controller.start(pollIntervalSeconds: 0.40)
    }

    private func updateMenuCounters() {
        sessionItem.title = "Session affected chars: \(controller.sessionAffected)"
        allTimeItem.title = "All time affected chars: \(controller.allTimeAffected)"
    }

    @objc private func toggleEnabled(_ sender: NSMenuItem) {
        controller.enabled.toggle()
        sender.state = controller.enabled ? .on : .off
    }

    @objc private func quit(_ sender: Any?) {
        store.allTimeAffectedChars = controller.allTimeAffected
        store.save()
        NSApp.terminate(nil)
    }
}
