

export abstract class TestUtils {
    static formatDate(date: Date) {
        return date.toISOString().split('T')[0];
    }    
} 