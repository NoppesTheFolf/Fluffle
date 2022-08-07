import { DateTime } from 'luxon'

export default class ShortUuidDateTime {
    static fromString(id: string) {
        let alphabet = {};
        Array.from('ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789').forEach((char, i) => alphabet[char] = i);

        let year = alphabet[id[0]] + 2020;
        let month = alphabet[id[1]];
        let day = alphabet[id[2]];
        let hour = alphabet[id[3]];
        let minute = alphabet[id[4]] * 2;
        let dateTime = DateTime.fromFormat(`${year} ${month} ${day} ${hour} ${minute}`, 'y L d H m', { zone: 'utc' });

        return dateTime;
    }
}
