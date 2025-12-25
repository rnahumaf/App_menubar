import Foundation

final class StatsStore {
    private static let keyAllTime = "allTimeAffectedChars"

    var allTimeAffectedChars: Int64

    init(allTimeAffectedChars: Int64 = 0) {
        self.allTimeAffectedChars = allTimeAffectedChars
    }

    static func load() -> StatsStore {
        let v = UserDefaults.standard.object(forKey: keyAllTime) as? NSNumber
        return StatsStore(allTimeAffectedChars: v?.int64Value ?? 0)
    }

    func save() {
        UserDefaults.standard.set(NSNumber(value: allTimeAffectedChars), forKey: Self.keyAllTime)
    }
}
